import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'
import webpush from 'npm:web-push@3.6.7'

const supabaseUrl = Deno.env.get('SUPABASE_URL')!
const supabaseServiceKey = Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')!
const vapidPublicKey = Deno.env.get('VAPID_PUBLIC_KEY')!
const vapidPrivateKey = Deno.env.get('VAPID_PRIVATE_KEY')!
const vapidSubject = Deno.env.get('VAPID_SUBJECT')!

webpush.setVapidDetails(vapidSubject, vapidPublicKey, vapidPrivateKey)

const supabase = createClient(supabaseUrl, supabaseServiceKey)

const corsHeaders = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'POST, OPTIONS',
    'Access-Control-Allow-Headers': 'Authorization, Content-Type'
}

interface SowingRow {
    id: number
    user_id: string
    plant_name: string
    status: number
    status_updated_at: string | null
    sown_date: string
}

interface PushSubscriptionRow {
    user_id: string
    subscription_json: string
}

function getDaysStale(statusUpdatedAt: string | null, sownDate: string): number {
    const lastUpdate = statusUpdatedAt ? new Date(statusUpdatedAt) : new Date(sownDate)
    const now = new Date()
    return Math.floor((now.getTime() - lastUpdate.getTime()) / (1000 * 60 * 60 * 24))
}

function getWarning(status: number, days: number): string | null {
    switch (status) {
        case 0: return days > 21 ? `Har inte grott på ${days} dagar — groplats, fukt och ljus rätt?` : null
        case 1: return days > 14 ? `Har stått på Grodd i ${days} dagar — dags att kolla karaktärsblad?` : null
        case 2: return days > 21 ? `Har haft karaktärsblad i ${days} dagar — dags att omskola?` : null
        case 3:
        case 4: return days > 14 ? `Omskolad för ${days} dagar sedan — dags att börja avhärda?` : null
        case 5: return days > 14 ? `Avhärdas sedan ${days} dagar — redo att plantera ut?` : null
        default: return null
    }
}

Deno.serve(async (req) => {
    // Hantera CORS preflight
    if (req.method === 'OPTIONS') {
        return new Response(null, { headers: corsHeaders })
    }

    try {
        const { data: sowings, error: sowingsError } = await supabase
            .from('v_user_sowings')
            .select('id, user_id, plant_name, status, status_updated_at, sown_date')
            .lt('status', 6)

        if (sowingsError) throw sowingsError

        const { data: subscriptions, error: subsError } = await supabase
            .from('push_subscriptions')
            .select('user_id, subscription_json')

        if (subsError) throw subsError

        const sowingsByUser = new Map<string, SowingRow[]>()
        for (const sowing of sowings as SowingRow[]) {
            if (!sowingsByUser.has(sowing.user_id)) {
                sowingsByUser.set(sowing.user_id, [])
            }
            sowingsByUser.get(sowing.user_id)!.push(sowing)
        }

        let notificationsSent = 0

        for (const sub of subscriptions as PushSubscriptionRow[]) {
            const userSowings = sowingsByUser.get(sub.user_id) || []
            const staleSowings = userSowings.filter(s => {
                const days = getDaysStale(s.status_updated_at, s.sown_date)
                return getWarning(s.status, days) !== null
            })

            if (staleSowings.length === 0) continue

            const subscription = JSON.parse(sub.subscription_json)

            const payload = JSON.stringify({
                title: `🌱 ${staleSowings.length} sådder behöver uppmärksamhet`,
                body: staleSowings
                    .map(s => {
                        const days = getDaysStale(s.status_updated_at, s.sown_date)
                        return `${s.plant_name}: ${getWarning(s.status, days)}`
                    })
                    .join('\n'),
                url: '/sowings'
            })

            try {
                await webpush.sendNotification(subscription, payload)
                notificationsSent++
                console.log(`Notis skickad till användare ${sub.user_id}`)
            } catch (e) {
                console.error(`Kunde inte skicka notis till ${sub.user_id}:`, e)
                if (e.statusCode === 410) {
                    await supabase
                        .from('push_subscriptions')
                        .delete()
                        .eq('user_id', sub.user_id)
                }
            }
        }

        return new Response(
            JSON.stringify({ success: true, notificationsSent }),
            { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
        )

    } catch (error) {
        console.error('Fel i Edge Function:', error)
        return new Response(
            JSON.stringify({ error: error.message }),
            { status: 500, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
        )
    }
})