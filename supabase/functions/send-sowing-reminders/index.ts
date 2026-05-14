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

interface NotificationSettingsRow {
    user_id: string
    enabled: boolean
    days_inactive_reminder: number
}

function getDaysStale(statusUpdatedAt: string | null, sownDate: string): number {
    const lastUpdate = statusUpdatedAt ? new Date(statusUpdatedAt) : new Date(sownDate)
    const now = new Date()
    return Math.floor((now.getTime() - lastUpdate.getTime()) / (1000 * 60 * 60 * 24))
}

function getStaleSowingMessage(status: number, days: number, plantName: string): string | null {
    switch (status) {
        case 0: return `${plantName}: Har inte grott på ${days} dagar — kontrollera temperatur, fukt och ljus?`
        case 1: return `${plantName}: Groddstadiet i ${days} dagar — dags att kolla karaktärsblad?`
        case 2: return `${plantName}: Karaktärsblad i ${days} dagar — dags att skolning?`
        case 3: return `${plantName}: Omskolad för ${days} dagar sedan — dags att börja avhärda?`
        case 4: return `${plantName}: Avhärdas i ${days} dagar — redo att plantera ut?`
        case 5: return `${plantName}: Utplanterad för ${days} dagar sedan — dags att registrera skörd?`
        default: return null
    }
}

Deno.serve(async (req) => {
    if (req.method === 'OPTIONS') {
        return new Response(null, { headers: corsHeaders })
    }

    try {
        const [sowingsResult, subscriptionsResult, settingsResult] = await Promise.all([
            supabase
                .from('v_user_sowings')
                .select('id, user_id, plant_name, status, status_updated_at, sown_date')
                .lte('status', 6),
            supabase
                .from('push_subscriptions')
                .select('user_id, subscription_json'),
            supabase
                .from('notification_settings')
                .select('user_id, enabled, days_inactive_reminder')
        ])

        if (sowingsResult.error) throw sowingsResult.error
        if (subscriptionsResult.error) throw subscriptionsResult.error
        // settings errors are non-fatal — users without a row get defaults

        const sowings = sowingsResult.data as SowingRow[]
        const subscriptions = subscriptionsResult.data as PushSubscriptionRow[]
        const settingsByUser = new Map<string, NotificationSettingsRow>(
            ((settingsResult.data ?? []) as NotificationSettingsRow[]).map(s => [s.user_id, s])
        )

        const sowingsByUser = new Map<string, SowingRow[]>()
        for (const sowing of sowings) {
            if (!sowingsByUser.has(sowing.user_id)) sowingsByUser.set(sowing.user_id, [])
            sowingsByUser.get(sowing.user_id)!.push(sowing)
        }

        let notificationsSent = 0

        for (const sub of subscriptions) {
            // Respect per-user notification settings (default: enabled, 14-day threshold)
            const settings = settingsByUser.get(sub.user_id)
            if (settings && !settings.enabled) continue

            const inactiveThreshold = settings?.days_inactive_reminder ?? 14

            const userSowings = sowingsByUser.get(sub.user_id) ?? []
            const staleSowings = userSowings.filter(s => {
                const days = getDaysStale(s.status_updated_at, s.sown_date)
                return days >= inactiveThreshold && getStaleSowingMessage(s.status, days, s.plant_name) !== null
            })

            if (staleSowings.length === 0) continue

            const subscription = JSON.parse(sub.subscription_json)
            const payload = JSON.stringify({
                title: `🌱 ${staleSowings.length} sådd${staleSowings.length === 1 ? '' : 'er'} behöver uppmärksamhet`,
                body: staleSowings
                    .slice(0, 3)
                    .map(s => getStaleSowingMessage(s.status, getDaysStale(s.status_updated_at, s.sown_date), s.plant_name))
                    .join('\n'),
                url: '/sowings'
            })

            try {
                await webpush.sendNotification(subscription, payload)
                notificationsSent++
            } catch (e) {
                console.error(`Could not send notification to ${sub.user_id}:`, e)
                if (e.statusCode === 410) {
                    await supabase.from('push_subscriptions').delete().eq('user_id', sub.user_id)
                }
            }
        }

        return new Response(
            JSON.stringify({ success: true, notificationsSent }),
            { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
        )

    } catch (error) {
        console.error('Error in Edge Function:', error)
        return new Response(
            JSON.stringify({ error: error.message }),
            { status: 500, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
        )
    }
})
