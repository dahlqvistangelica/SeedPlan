# SeedPlan – Specifikation v2.0

> Baserad på befintlig app (Blazor WebAssembly + Supabase) – mars 2026
> Odlingsplatser är borttagna. Fokus på att bygga vidare på det som redan finns.

---

## 1. Översikt

SeedPlan är en PWA för att hantera fröinventering, planera sådder baserat på sista frostdatum, följa såddars utveckling steg för steg och analysera resultat över säsonger. All data lagras i Supabase med individuella användarkonton.

### Befintlig stack (ändras ej)

| Egenskap | Val |
|---|---|
| Typ | PWA – Blazor WebAssembly (.NET 8) |
| Databas & Auth | Supabase (PostgreSQL + GoTrue) |
| Deployment | Docker + Railway |
| Språk | Svenska UI |

### PWA-krav (befintliga + utökade)
- Installerbar på hemskärm via `manifest.webmanifest` ✅ (finns)
- Offline-läsning av inventarie och aktiva såddar via Service Worker ✅ (finns, grundläggande)
- Push-notiser via Web Push API ✅ (grundinfrastruktur finns – se sektion 8)
- Responsiv design, primärt mobilvy ✅ (finns)

---

## 2. Autentisering & Kontoinställningar

### 2.1 Befintligt (ändras ej)
- Supabase Auth med e-post + lösenord
- Registrering med för- och efternamn
- Inloggning med felmeddelanden på svenska
- Row Level Security – all användardata är strikt isolerad

### 2.2 Nya kontofunktioner (ska byggas)

Dessa funktioner läggs till på **Inställningssidan** (`/profile`) i en ny sektion "Kontoinställningar".

#### Ändra e-postadress
- Användaren anger ny e-postadress + bekräftar med nuvarande lösenord
- Supabase skickar bekräftelsemejl till den nya adressen
- Adressen uppdateras först när länken klickas
- Visa tydlig statustext: *"Ett bekräftelsemejl har skickats till [ny adress]."*

#### Ändra lösenord
- Användaren anger nuvarande lösenord + nytt lösenord + bekräfta nytt lösenord
- Validering: minst 8 tecken, lösenorden matchar
- Felmeddelanden på svenska (fel nuvarande lösenord, för svagt lösenord)
- Visa bekräftelse vid lyckad ändring

#### Radera konto
- Placeras längst ned på inställningssidan, visuellt tydligt separerad (t.ex. röd sektion eller röd knapp)
- **Steg 1:** Knapp "Radera mitt konto" → öppnar bekräftelsedialog
- **Steg 2:** Användaren måste skriva in sitt lösenord för att bekräfta
- **Steg 3:** All användardata raderas (seeds, sowings, sowing_events, user_profiles, user_plants, tags) via Supabase-funktion eller cascade delete
- **Steg 4:** Auth-kontot raderas via Supabase Admin API (kräver Edge Function)
- Användaren loggas ut och skickas till startsidan med meddelandet: *"Ditt konto har raderats."*
- **Affärsregel:** Kontot kan inte återställas efter radering.

#### Exportera/Backup data
- Knapp: "Ladda ned min data (JSON)"
- Exporterar all användardata som en enda JSON-fil: profil, frön, såddar, händelseloggar, egna växter
- Genereras på klientsidan från befintliga service-anrop (ingen ny backend-endpoint behövs)
- Filnamn: `seedplan-backup-YYYY-MM-DD.json`
- Ingen import-funktion i v2 (backup är read-only)

---

## 3. Profil & Inställningar (utökad)

### Befintliga fält (ändras ej)
- Namn
- Odlingszon (1–8)
- Sista frostdatum

### Nya fält
- **Ort** (fritext, t.ex. "Göteborg") – används endast som visningsinformation, påverkar inga beräkningar

### Sidans nya struktur
```
Inställningar
├── Trädgårdsinställningar     (befintliga fält + ort)
├── Kontoinställningar         (ändra e-post, ändra lösenord)
├── Notiser                    (ny sektion – se sektion 8)
└── Farozon                    (radera konto, exportera data)
```

---

## 4. Fröinventariet (utökat)

### 4.1 Befintliga fält (ändras ej)
- Art (koppling till växtdatabas)
- Variant/sort (fritext)
- Antal frön
- Utgångsdatum
- Anteckningar

### 4.2 Nya fält (läggs till i AddSeedModal och EditSeedModal)

| Fält | Typ | Notering |
|---|---|---|
| Inköpsdatum | Datum | Valfritt |
| Inköpsställe | Fritext | T.ex. "Runåbergs fröer" |
| Grobarhetstestresultat | Heltal 0–100 | Procent, valfritt |
| Taggar | Många-till-många | Se sektion 4.3 |

### 4.3 Taggar
- Användaren skapar egna taggar (t.ex. "favorit", "ekologisk", "ny")
- Taggar kopplas till frön (ett frö kan ha flera taggar)
- Taggar visas som klickbara chips på frökortet
- Filtrering på tagg möjlig i frövisningen

### 4.4 Varningar i inventariet
- **Gult** märke: Utgångsdatum inom 6 månader
- **Rött** märke: Utgångsdatum passerat
- **Grått/tonat** kort: Lagersaldo = 0 (frö visas men markeras som slut)

### 4.5 Filtrering & sökning (utökad)
Befintlig sökning och gruppering kompletteras med:
- Filtrera på tagg (en eller flera)
- Sortering: art A–Ö, utgångsdatum (närmast först), antal kvar (minst först), senast tillagd
- Visa/dölj frön med saldo 0

### 4.6 Lagerregler
- Lagret får aldrig gå under noll – sådd blockeras med felmeddelande ✅ (finns delvis, säkerställs)
- Frön återförs till lagret om en sådd raderas **innan** status "Groddning" nåtts
- Efter "Groddning": frön återförs inte vid radering (de är förbrukade)
- Om en sådd ångras/raderas och status ≥ Groddning visas ett informationsmeddelande: *"Fröna återförs inte till lagret eftersom sådden redan grott."*

---

## 5. Växtdatabas

### 5.1 Befintlig databas (ändras ej i struktur)
Befintlig `plants`-tabell med fälten:
- `plant_name`, `scientific_name`, `category`, `hardiness_level`
- `sowing_lead_time` (veckor), `weeks_before_frost`
- `is_light_germinating`, `requires_topping`, `direct_sowing`
- `sowing_depth_mm`, `plant_spacing_cm`, `dev_time_min`, `dev_time_max`

### 5.2 Saknade fält som läggs till i databasen

| Fält | Typ | Beskrivning |
|---|---|---|
| `germination_days_min` | int | Minsta förväntade groddningstid i dagar |
| `germination_days_max` | int | Högsta förväntade groddningstid i dagar |
| `days_to_harvest` | int | Ungefärliga dagar från sådd till skörd |
| `sowing_notes` | text | Generella odlingstips om arten |

### 5.3 Egna växter (`user_plants`) – ny funktion
- Användaren kan lägga till egna arter som inte finns i databasen
- Samma fält som i `plants`-tabellen
- Kan baseras på en befintlig art som mall ("forka" en art)
- Visas och beter sig identiskt med inbyggda arter i alla delar av appen
- Märks med en liten ikon i växtguiden för att skilja dem åt

---

## 6. Såddrekommendationer (utökad logik)

### 6.1 Befintlig logik (fungerar, men utökas)
Nuvarande logik visar arter vars såddfönster är aktivt idag (±14 dagar). Fönstret beräknas som:
```
Sådatum = Sista frostdatum − (SowingLeadTime × 7 dagar)
Fönster = Sådatum till Sådatum + 14 dagar
```

### 6.2 Utökad beräkningslogik

**Inomhussådd (försprång) – nuvarande typ:**
```
Rekommenderat startdatum = Sista frostdatum − (försprångsveckor × 7)
Optimalt fönster = startdatum ± 7 dagar
Sista chans = startdatum + 14 dagar
```

**Direktsådd utomhus (`direct_sowing = true`):**
```
Rekommenderat datum = Sista frostdatum + (weeks_before_frost × 7)
(negativt weeks_before_frost = härdiga arter som kan sås FÖRE sista frost)
```

**Brådskegrader (används för sortering och färgkodning):**
- 🟢 Grön – optimalt: inom ±7 dagar från rekommenderat datum
- 🟡 Gul – ok men inte idealt: 8–14 dagar från rekommenderat datum
- 🔴 Röd – sista chans: 15–21 dagar efter rekommenderat datum
- ⚫ Grå – passerat: >21 dagar efter rekommenderat datum (visas i "Redan passerat"-sektionen)

**Kommande (visas i "Snart att så"):**
- Arter vars såddfönster börjar inom de närmaste 14 dagarna

### 6.3 Presentationsvyer

**Dashboardvyn (befintlig, utökad):**
- "Bör sås nu" – aktiva fönster, sorterade på brådska, med färgkodning ✅ (finns, utökas med färger)
- "Snart att så" – ny sektion för kommande 14 dagar
- "Redan passerat" – befintlig accordion-sektion ✅

**Listvy på egen sida (`/planning`) – ny sida:**
- Fullständig lista med alla arter och deras såddstatus för säsongen
- Filter: visa bara de med frön i lager / visa alla
- Sortering: brådska, art A–Ö, datum
- Varje rad visar: art, rekommenderat datum, dagar kvar/sedan, brådskegrad, om frön finns i lager

**Kalendervy (fas 3 – ingår ej i v2):**
- Månadskalender med färgkodade prickar per art

### 6.4 Koppling till inventariet
- Om användaren har frön av arten i lager → visa "Så nu"-knapp
- Om frön saknas → nedtonad rad med ikon och text "Frö saknas"
- Klick på art → öppnar PlantDetailModal (befintlig)

---

## 7. Såddhantering (utökad)

### 7.1 Befintligt statusflöde (ändras, utökas)

**Nuvarande statuskoder (int):**
```
0 = Sådd, 1 = Groddning, 2 = Karaktärsblad, 3/4 = Omskolning, 5 = Avhärdning, 6 = Utplanterad
```

**Nytt statusflöde:**
```
0 Sådd
→ 1 Groddning
→ 2 Karaktärsblad
→ 3 Omskolning
→ 4 Avhärdning
→ 5 Utplanterad
→ 6 Skörd          (ny)
→ 7 Avslutad        (ny)
   eller
→ 99 Misslyckad     (ny, kan nås från vilket steg som helst)
```

Obs: befintliga statuskoder 3 och 4 (Omskolning PottedOn1/PottedOn2) slås ihop till ett steg (3) för enklare UX. Databasens `status`-värden justeras med en migration.

### 7.2 Omgångsnummer – ny funktion
- Flera aktiva såddar av samma frö/art tillåts
- Omgångsnummer föreslås automatiskt (nästa lediga för det fröet)
- Visas som "Tomat Sungold – Omgång 1", "Omgång 2"
- Nytt fält `batch_number int` på `sowings`-tabellen

### 7.3 Händelselogg per sådd – ny funktion
Varje statusövergång loggas i en ny tabell `sowing_events`:

| Fält | Typ | Beskrivning |
|---|---|---|
| `id` | int | PK |
| `sowing_id` | int | FK → sowings |
| `user_id` | uuid | För RLS |
| `event_type` | text | Statusvärdet som nåtts |
| `event_date` | date | Datum för händelsen |
| `seedlings_count` | int? | Antal groddar/plantor (vid groddning/uppkomst) |
| `harvest_weight_g` | int? | Vikt i gram (vid skörd) |
| `harvest_count` | int? | Antal skördade (vid skörd) |
| `notes` | text? | Fritext |

**Vad loggas automatiskt vs manuellt:**
- Statusövergång 0→1 (Groddning): frågar om antal groddar synliga
- Statusövergång till 6 (Skörd): frågar om vikt eller antal
- Statusövergång till 99 (Misslyckad): frågar om orsak (fritext, valfritt)
- Övriga övergångar: loggas med datum, inga extra fält krävs

### 7.4 Radera en sådd (uppdaterad logik)
- Bekräftelsedialog ✅ (finns)
- Status < Groddning (0): frön återförs till lagret automatiskt
- Status ≥ Groddning (1+): frön återförs inte, men information visas för användaren
- Händelseloggar raderas via cascade delete

### 7.5 Misslyckad sådd
- Knapp "Markera som misslyckad" tillgänglig på alla aktiva såddar
- Dialog öppnas med valfritt fritext-fält för orsak
- Frön som dragits från lagret återförs **inte** (de är förbrukade)
- Misslyckade såddar visas separat i såddarnas listvy (filtrerbart)

---

## 8. Notissystem

### 8.1 Vad som redan finns ✅

**Klientsidan (`NotificationService.cs` + `notifications.js`):**
- `requestNotificationPermission` – begär webbläsartillstånd vid inloggning
- `subscribeToPush` – registrerar push-subscription och sparar till `push_subscriptions`-tabellen i Supabase
- `CheckAndNotifyStaleAsync` – kontrollerar orörda såddar och skickar lokala notiser vid appstart
- Stale-logik i `SowingHelper.GetStaleWarning` – trösklar per status (0→10 dagar, 1→14, 2→21, 3/4→14, 5→14)
- Notiser visas även som varningskort på Dashboard och Såddar-vyn

**Serversidan (Supabase Edge Function `send-sowing-reminders`):**
- Hämtar alla aktiva såddar (status < 6) och kör stale-logiken
- Skickar Web Push via `web-push`-biblioteket med VAPID-nycklar
- Rensar automatiskt utgångna subscriptions (HTTP 410)
- Kan triggas manuellt via HTTP POST

**Databasstruktur:**
- `push_subscriptions`-tabell finns (`user_id`, `subscription_json`, `updated_at`)

### 8.2 Vad som saknas och ska byggas

**Automatisk daglig trigger:**
- Edge Function finns men saknar schemalagd körning via pg_cron eller Supabase Scheduled Functions
- Kräver att ett cron-jobb sätts upp i Supabase Dashboard eller via SQL: `select cron.schedule(...)`

**Notiser för såddrekommendationer (ny typ):**
- Befintlig Edge Function hanterar bara orörda såddar, inte kommande såddfönster
- Ska utökas med: för varje användare, beräkna såddfönster → skicka påminnelse X dagar i förväg
- Triggerlogik: `rekommenderat_datum - idag <= X dagar` (X konfigureras i `notification_settings`)

**Inställningsgränssnitt (saknas helt):**
- Sektion "Notiser" på Inställningssidan saknas i UI
- Ska innehålla: global av/på-switch, konfigurerbart antal dagar för påminnelse

**`notification_settings`-tabell saknas:**
```sql
CREATE TABLE notification_settings (
  id serial PRIMARY KEY,
  user_id uuid REFERENCES auth.users NOT NULL UNIQUE,
  enabled boolean DEFAULT true,
  days_before_sowing int[] DEFAULT '{7,2}',
  days_inactive_reminder int DEFAULT 14
);
```

### 8.3 Notistyper (totalt)

| Typ | Status | Trigger | Innehåll |
|---|---|---|---|
| Orörd sådd | ✅ finns | Stale-kontroll vid appstart + Edge Function | "Har [Art] grott? Den har stått orörd i X dagar." |
| Kommande såddfönster | 🔨 ska byggas | Edge Function-utökning | "Dags snart att så [Art]! Rekommenderat datum: [datum]." |
| Notiskonfiguration i UI | 🔨 ska byggas | – | Inställningar på /profile |

---

## 9. Statistik & Analys – ny funktion

### 9.1 Ny navigationssektion (`/statistics`)
Ny flik i bottom navigation ersätter eller läggs till bredvid befintliga.

### 9.2 Per art/variant

- **Groningsprocent:** `(antal groddar vid groddningshändelse / antal sådda) × 100`
  Beräknas från `sowing_events` – kräver att händelselogg finns
- **Dagar från sådd till skörd:** snitt och spann (min–max) för avslutade såddar
- **Rankingvy:** Arter sorterade på groningsprocent, topp 5 och botten 5

### 9.3 Säsongsöversikt

- Totalt antal sådder detta år
- Total groningsprocent hittills
- Antal avslutade såddar, antal misslyckade
- Bästa art (högst groningsprocent)

### 9.4 Inventariestatistik (dashboardwidgets)

- Totalt antal frösorter i lager
- Antal sorter med utgångsdatum inom 6 månader
- Antal sorter med saldo = 0

### 9.5 Krav för statistik
Statistik bygger på händelseloggar (`sowing_events`). Om en användare inte har loggat mätvärden visas ett tomt tillstånd med uppmaning: *"Logga mätvärden när dina såddar gror för att se statistik här."*

---

## 10. Databasschema (delta från befintligt)

Befintliga tabeller som **ändras**:

```sql
-- Lägg till fält på user_profiles
ALTER TABLE user_profiles ADD COLUMN location_name text;
-- OBS: push_subscription sparas INTE i user_profiles utan i separat push_subscriptions-tabell (finns redan)

-- Lägg till fält på plants (inbyggd växtdatabas)
ALTER TABLE plants ADD COLUMN germination_days_min int;
ALTER TABLE plants ADD COLUMN germination_days_max int;
ALTER TABLE plants ADD COLUMN days_to_harvest int;
ALTER TABLE plants ADD COLUMN sowing_notes text;

-- Lägg till fält på seeds
ALTER TABLE seeds ADD COLUMN purchase_date date;
ALTER TABLE seeds ADD COLUMN purchase_location text;
ALTER TABLE seeds ADD COLUMN germination_rate int CHECK (germination_rate >= 0 AND germination_rate <= 100);

-- Lägg till fält på sowings
ALTER TABLE sowings ADD COLUMN batch_number int DEFAULT 1;
```

Nya tabeller (push_subscriptions finns redan ✅):

```sql
-- Egna växter
CREATE TABLE user_plants (
  id serial PRIMARY KEY,
  user_id uuid REFERENCES auth.users NOT NULL,
  based_on_plant_id int REFERENCES plants,   -- null om helt eget
  plant_name text NOT NULL,
  scientific_name text,
  category int NOT NULL,                      -- samma enum som plants
  hardiness_level int NOT NULL DEFAULT 0,
  sowing_lead_time int NOT NULL DEFAULT 8,
  weeks_before_frost int NOT NULL DEFAULT 0,
  is_light_germinating boolean DEFAULT false,
  requires_topping boolean DEFAULT false,
  direct_sowing boolean DEFAULT false,
  sowing_depth_mm float,
  plant_spacing_cm int,
  germination_days_min int,
  germination_days_max int,
  days_to_harvest int,
  sowing_notes text,
  created_at timestamptz DEFAULT now()
);

-- Taggar
CREATE TABLE tags (
  id serial PRIMARY KEY,
  user_id uuid REFERENCES auth.users NOT NULL,
  name text NOT NULL
);

-- Taggkoppling till frön
CREATE TABLE seed_tags (
  seed_id int REFERENCES seeds ON DELETE CASCADE,
  tag_id int REFERENCES tags ON DELETE CASCADE,
  PRIMARY KEY (seed_id, tag_id)
);

-- Händelselogg per sådd
CREATE TABLE sowing_events (
  id serial PRIMARY KEY,
  sowing_id int REFERENCES sowings ON DELETE CASCADE NOT NULL,
  user_id uuid REFERENCES auth.users NOT NULL,
  event_type text NOT NULL,
  event_date date NOT NULL DEFAULT current_date,
  seedlings_count int,
  harvest_weight_g int,
  harvest_count int,
  notes text,
  created_at timestamptz DEFAULT now()
);

-- Notiskonfiguration
CREATE TABLE notification_settings (
  id serial PRIMARY KEY,
  user_id uuid REFERENCES auth.users NOT NULL UNIQUE,
  enabled boolean DEFAULT false,
  days_before_sowing int[] DEFAULT '{7,2}',
  days_inactive_reminder int DEFAULT 14
);
```

**RLS-policies:** Alla nya tabeller får policies med `auth.uid() = user_id`.

---

## 11. UI-struktur (uppdaterad navigation)

```
📱 Bottom navigation:
├── 🏠 Dashboard      /          – Sårekommendationer + aktiva såddar + varningar
├── 🌱 Fröer          /seeds     – Frölagret (befintlig, utökad)
├── 🌿 Såddar         /sowings   – Aktiva och avslutade såddar (befintlig, utökad)
├── 📖 Guide          /guide     – Växtguide (befintlig)
└── ⚙️ Inställningar  /profile   – Profil + kontoinställningar (utökad)
```

Ny sida (nås från dashboard eller direkt):
```
├── 📅 Planering      /planning  – Fullständig såddlista för säsongen
└── 📊 Statistik      /statistics – Groningsprocent, säsongsöversikt
```

Planering och Statistik läggs inte i bottom nav initialt (för att hålla navbaren ren) utan nås via knappar från dashboard.

---

## 12. Affärsregler (uppdaterade)

1. Lagret får aldrig gå under noll – sådd blockeras med felmeddelande
2. Frön återförs till lagret om sådd raderas **innan** status Groddning (status < 1)
3. Frön återförs **inte** om sådd raderas vid status ≥ Groddning – informationstext visas
4. Sista frostdatum är globalt per konto
5. Odlingsplatser finns inte – all beräkning sker mot globalt frostdatum
6. Flera såddar av samma art är tillåtet – identifieras med omgångsnummer
7. Statistik beräknas alltid från faktiska loggade händelser, aldrig uppskattningar
8. All användardata är strikt isolerad via Supabase RLS
9. Kontoradering är permanent och kan inte ångras
10. Backup-export är read-only – ingen import-funktion i v2

---

## 13. Byggas i denna prioritetsordning

### Prioritet 1 – Kontofunktioner (låg risk, hög nytta)
- Ändra lösenord
- Ändra e-postadress
- Exportera data (JSON-backup)
- Radera konto

### Prioritet 2 – Utökat fröinventarie
- Nya fält: inköpsdatum, inköpsställe, grobarhetsprocent
- Varningsindikatorer (gult/rött för utgångsdatum, grått för saldo 0)
- Taggsystem

### Prioritet 3 – Förbättrade såddrekommendationer
- Brådskegrader och färgkodning
- "Snart att så"-sektion på dashboarden
- Direktsådd-logik för `direct_sowing = true`
- Planeringssida (`/planning`)

### Prioritet 4 – Utökad såddhantering
- Omgångsnummer
- Nytt statusflöde med Skörd/Avslutad/Misslyckad
- Händelselogg (`sowing_events`)
- Korrekt återföringslogik vid radering

### Prioritet 5 – Statistik
- Statistiksida (`/statistics`)
- Groningsprocent per art
- Säsongsöversikt

### Prioritet 6 – Notiser (delvis klart)
- ✅ Push-infrastruktur (NotificationService, notifications.js, push_subscriptions-tabell)
- ✅ Stale-notiser vid appstart och via Edge Function
- ✅ Edge Function `send-sowing-reminders` (manuell trigger)
- 🔨 Sätt upp automatisk daglig cron-trigger i Supabase
- 🔨 Utöka Edge Function med såddrekommendations-notiser
- 🔨 Notiskonfiguration i inställningsgränssnittet (`notification_settings`-tabell + UI)

---

## 14. Ingår ej i v2 (framtida fas)

- Kalendervy för såddplanering (månadskalender med prickar)
- Adaptiva rekommendationer baserade på historik
- Bildlogg per sådd (Supabase Storage)
- Export till PDF/CSV
- Delning av data mellan användare
- Väderprognos-integration
- Egna växter (`user_plants`) – komplex, sparas till fas 3
- Import av backup-data

---

*Specifikation v2.0 – SeedPlan – mars 2026*