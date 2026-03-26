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

### 2.2 Kontofunktioner (status + mål)

Statusmarkeringar i denna sektion:
- ✅ klart
- 🟡 delvis klart
- 🔨 saknas

Nuvarande app har både en **Inställningsöversikt** (`/settings`) och en **Profilsida** (`/profile`).
Måldesignen i v2 är att kontorelaterade flöden är samlade och tydligt sektionerade på inställningsflödet.

#### Ändra e-postadress
- **Status:** ✅ klart
- ✅ UI och service för ny e-post finns
- ✅ Supabase-flöde med bekräftelsemejl används
- ✅ Bekräftelse med nuvarande lösenord krävs i UI-flödet
- ✅ Statustext förtydligad: *"Ett bekräftelsemejl har skickats till [ny adress]."*

#### Teknisk notering (auth)
- AuthService använder en intern auth-abstraktion (`IAuthClient`) i stället för direkt mockning av Supabase-klienten.
- Syfte: göra enhetstester stabila utan att ändra användarflöden i appen.
- Unit-testtäckningen är utökad för auth-flöden, inklusive negativa vägar för e-post/lösenordsuppdatering (fel nuvarande lösenord och misslyckad update), samt login edge-case när sessionspayload saknas.

#### Ändra lösenord
- **Status:** ✅ klart
- ✅ Flöde finns med nuvarande lösenord + nytt lösenord + bekräftelse
- ✅ Felmeddelanden visas på svenska
- ✅ Bekräftelse visas vid lyckad ändring
- ✅ Validering är konsekvent och implementerad i både UI och service:
  - minst 8 tecken
  - minst en stor bokstav
  - minst en liten bokstav
  - matchande bekräftelselösenord i UI

**Acceptanskriterier (Given/When/Then)**
- **Given** att användaren är inloggad och anger korrekt nuvarande lösenord, **When** nytt lösenord uppfyller alla krav och bekräftelsen matchar, **Then** lösenordet uppdateras och lyckad status visas.
- **Given** att nytt lösenord är kortare än 8 tecken, **When** användaren försöker spara, **Then** uppdatering blockeras och felmeddelande om minsta längd visas.
- **Given** att nytt lösenord saknar stor bokstav, **When** användaren försöker spara, **Then** uppdatering blockeras och felmeddelande om stor bokstav visas.
- **Given** att nytt lösenord saknar liten bokstav, **When** användaren försöker spara, **Then** uppdatering blockeras och felmeddelande om liten bokstav visas.
- **Given** att bekräftelselösenord inte matchar, **When** användaren försöker spara, **Then** UI blockerar skick och visar varning om att lösenorden inte matchar.
- **Given** att nuvarande lösenord är felaktigt, **When** användaren försöker spara ett giltigt nytt lösenord, **Then** uppdatering avvisas med felmeddelande om felaktigt nuvarande lösenord.

#### Radera konto
- Flyttad till framtida fas (se sektion 14).

#### Exportera/Backup data
- Flyttad till framtida fas (se sektion 14).

---

## 3. Profil & Inställningar (utökad)

### 3.0 Nuvarande läge (mars 2026)
- `/settings` används som inställningsöversikt (frostdatum, zon, aviseringar, länkar till profil).
- `/profile` innehåller profil- och kontouppgifter (namn, e-postbyte, lösenordsbyte).
- `/profile` innehåller även kategorival (checkboxar) för startsidans såförslag.
- Frostdatum och odlingszon hanteras som separata kort/modaler i `/settings`.
- Odlingszon visas inte i kontoinställningar i `/profile`.

### Befintliga fält (ändras ej)
- Namn
- Sista frostdatum
- Odlingszon (1–8)

### Nya fält
- **Ort** (fritext, t.ex. "Göteborg") – används endast som visningsinformation, påverkar inga beräkningar

### Sidans nya struktur
```
Inställningsflöde
├── Översikt                     (`/settings`)
│   ├── Frostdatum               (egen knapp + egen modal)
│   ├── Odlingszon/Plats         (egen knapp + egen modal)
│   ├── Aviseringar
│   └── Profil (länk till `/profile`)
└── Profil & Konto               (`/profile`)
├── Trädgårdsinställningar     (namn + ort)
├── Kontoinställningar         (ändra e-post, ändra lösenord)
├── Notiser                    (ny sektion – se sektion 8)
└── Farozon                    (flyttad till framtida fas)
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

### 6.5 Kategorival för förslag på startsidan (ny)
- Användaren kan välja vilka växtkategorier som ska ingå i förslag på startsidan (`/`).
- Valet utgår från befintlig enum `PlantCategory`:
  - `Vegetable`
  - `Flower`
  - `Herb`
  - `Fruit`
- UI-krav: multival (checkbox/toggle per kategori) i inställningsflödet.
- Standard: alla kategorier valda för nya användare.
- Regler:
  - Endast valda kategorier visas i "Bör sås nu" och "Snart att så".
  - "Redan passerat" följer samma kategorifilter.
  - Om inga kategorier är valda visas tomt tillstånd med uppmaning att välja minst en kategori.
  - Avbockad kategori ska tas bort ur `preferred_plant_categories` (inte enbart döljas i UI-state).
  - Dubbletter i `preferred_plant_categories` ska inte förekomma.

**Implementationsnotering (klar):**
- `preferred_plant_categories` hanteras som Postgres-array (`int[]`) i modellagret.
- Profil-sparning visar omedelbar status (`Sparar...`) och byter till bekräftelse/fel när svar kommit.
- Spara-knappen i `/profile` är disabled under pågående sparning för att förhindra dubbelskick.

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

**Inställningsgränssnitt (delvis):**
- 🟡 Global av/på för aviseringar finns redan i `/settings`
- 🔨 Konfigurerbart antal dagar för påminnelse saknas
- 🔨 Notisinställningar behöver samlas/sektioneras enligt målstrukturen i sektion 3

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
| Notiskonfiguration i UI | 🟡 delvis | Global toggle finns i `/settings`; konfiguration saknas | Inställningsflödet (`/settings` + `/profile`) |

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
ALTER TABLE user_profiles ADD COLUMN preferred_plant_categories int[] DEFAULT '{0,1,2,3}';
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
└── ⚙️ Inställningar  /settings  – Inställningsöversikt + länk till profil/konto (utökad)
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
9. Kategorifilter på startsidan styr vilka växtkategorier som visas i rekommendationssektionerna

---

## 13. Byggas i denna prioritetsordning

### Prioritet 1 – Kontofunktioner (låg risk, hög nytta)
- ✅ Ändra lösenord (klart: kravvalidering + tydliga felmeddelanden)
- ✅ Ändra e-postadress (klart: lösenordsbekräftelse + tydlig statustext)

### Prioritet 2 – Profil & inställningsstruktur
- ✅ Behåll separata modaler för frostdatum och odlingszon i `/settings` (infört)
- ✅ Odlingszon borttagen från kontoinställningar i `/profile` (infört)
- ✅ Lägg till kategorival (PlantCategory) för startsidans förslag i `/profile` (infört)

### Prioritet 3 – Utökad såddhantering
- Omgångsnummer
- Nytt statusflöde med Skörd/Avslutad/Misslyckad
- Händelselogg (`sowing_events`)
- Korrekt återföringslogik vid radering

#### Implementeringsplan (Prio 3)

**Etapp 3.1 – Datamodell & migrationer**
- Lägg till `batch_number` på `sowings` (default `1`).
- Skapa tabellen `sowing_events` enligt sektion 7.3.
- Lägg till RLS för `sowing_events` med `auth.uid() = user_id`.
- Ta fram datamigrering för statusnormalisering enligt sektion 7.1 (inklusive befintliga värden 3/4).
- Leverabel: migrationer körbara lokalt + i Supabase utan dataförlust.

**Etapp 3.2 – Backend/Service-lager**
- Uppdatera statusenum och mappning mellan UI-värden och DB-värden.
- Inför central metod för statusövergång (`CanTransition` + `UpdateSowingStatusAsync`).
- Skriv händelselogg automatiskt vid varje statusövergång.
- Hantera specialfall: groddning (antal), skörd (vikt/antal), misslyckad (orsak).
- Leverabel: enhetstester för giltiga/ogiltiga övergångar och loggskapande.

**Etapp 3.3 – UI: såddflöde och detaljer**
- Visa omgångsnummer konsekvent i listor/kort/detaljer.
- Uppdatera status-UI med nya steg: Skörd, Avslutad, Misslyckad.
- Lägg till dialogflöden för extra data vid groddning/skörd/misslyckad.
- Visa händelsehistorik i sådddetalj (datum, event, metadata, anteckning).
- Leverabel: komplett användarflöde från "Sådd" till "Avslutad/Misslyckad".

**Etapp 3.4 – Raderings- och lagerregler**
- Implementera raderingslogik enligt affärsregler i sektion 12:
  - status `< 1` → återför frön till lager
  - status `>= 1` → återför inte, visa info
- Säkerställ att händelser i `sowing_events` raderas via cascade.
- Leverabel: testfall för båda raderingsvägarna + verifierat lagersaldo.

**Etapp 3.5 – Kvalitetssäkring & release**
- Lägg till integrationstester för hela statuskedjan och återföringsregler.
- Verifiera bakåtkompatibilitet för befintliga såddar efter migration.
- Lägg till kort release-checklista (DB migration, smoke-test, rollback-plan).
- Leverabel: "Definition of Done" uppfylld och redo för produktion.

#### Definition of Done (Prio 3)
- Omgångsnummer fungerar i skapande, visning och sortering.
- Statusflödet stödjer Skörd, Avslutad och Misslyckad utan inkonsistenta tillstånd.
- Varje statusändring ger korrekt rad i `sowing_events`.
- Raderingslogik följer lagerregler exakt.
- Tester täcker kritiska flöden och passerar i CI.

#### Ticket-förslag (GitHub Issues) – Prio 3

#### Implementationsstatus (uppdaterad 2026-03-26)

Statusmarkeringar i denna sektion:
- ✅ klart
- 🟡 delvis klart
- 🔨 ej påbörjat/ej klart

Ticketstatus just nu:
- **P3-01: DB migration – `batch_number` + `sowing_events` + RLS** → ✅ klart
  - `batch_number` finns i schemaflödet.
  - `sowing_events` skapas och RLS-policy (`auth.uid() = user_id`) finns i migrationer.
  - Migrationerna är idempotenta för både tom och befintlig databas.
- **P3-02: Statusnormalisering och enum-mappning** → ✅ klart
  - Ny enum och statusmappning används konsekvent i klientkod.
  - Bakåtkompatibilitet för äldre värden hanteras i migrationen för statusnormalisering.
  - Enhetstest för mappning/övergångar passerar.
- **P3-03: Central statusmotor i service-lagret** → ✅ klart
  - `CanTransition` + `UpdateSowingStatusAsync(request)` används som central väg.
  - Wrappern `UpdateSowingStatus(id, status)` finns kvar för bakåtkompatibilitet.
  - Ogiltiga övergångar blockeras och returnerar tydliga fel.
- **P3-04: Händelseloggning vid statusändring** → ✅ klart
  - Klient och RPC skickar/stödjer metadata (`seedlings_count`, `harvest_weight_g`, `harvest_count`, `notes`).
  - `sowing_events` + RLS säkrade i migrationskedjan.
- **P3-05: UI – omgångsnummer i listor och detaljer** → ✅ klart
  - Omgångsnummer visas konsekvent i Såddar-vyn och i dashboardens såddrelaterade kort.
  - Nästa omgångsnummer sätts automatiskt vid ny sådd per frö.
  - Enhetstester verifierar beräkningen av nästa omgångsnummer.
- **P3-06: UI – nytt statusflöde + dialoger för specialfall** → ✅ klart
  - Nya steg och specialdialoger finns för `0 -> 1` (antal), `-> 6` (vikt/antal), `-> 99` (orsak).
  - UI använder inte optimistisk uppdatering och visar tydliga fel vid ogiltig transition/serverfel.
- **P3-07: Raderingslogik och lageråterföring** → ✅ klart
  - Radering följer regel: status `< 1` återför frön till lager, status `>= 1` återför inte.
  - UI visar tydlig information när återföring inte sker ("Fröna återförs inte till lagret eftersom sådden redan grott.").
  - Minimala enhetstester finns för raderingsregeln.
- **P3-08: Såddhistorik i detaljvy** → ✅ klart
  - Historik från `sowing_events` visas i detaljsektion per sådd.
  - Metadata (`seedlings_count`, `harvest_weight_g`, `harvest_count`, `notes`) renderas utan UI-fel när de finns.
  - Tom historik hanteras med tydligt tomt tillstånd.
- **P3-09: Integrationstester + release-checklista** → ✅ klart
  - CI innehåller nu sammanhängande workflowtester för statuskedja, ogiltigt hopp, `x -> 99` och raderingsregel.
  - Release-checklista med migration, smoke-test och rollback finns dokumenterad.


**P3-08: Såddhistorik i detaljvy**
- Scope:
  - Visa tidslinje/lista från `sowing_events` i sådddetalj
  - Rendera metadata och anteckningar per event
- Acceptanskriterier:
  - Historik visas i kronologisk ordning
  - Event utan metadata visas utan UI-fel
  - Event med metadata visar rätt värden

**P3-09: Integrationstester + release-checklista**
- Scope:
  - Lägg till testscenario från skapad sådd → avslutad/misslyckad → ev. radering
  - Dokumentera release-checklista och rollback
- Acceptanskriterier:
  - CI innehåller test som täcker hela Prio 3-kedjan
  - Checklista inkluderar migration, smoke-test, rollback-steg
  - Prio 3 kan verifieras mot Definition of Done

  **Release-checklista (Prio 3)**
  1. **Förberedelse**
    - Säkerställ backup/snapshot av databasen.
    - Verifiera att inga pågående manuella schemaändringar finns i målmiljön.
  2. **Migrering**
    - Kör migrationer i ordning till senaste version.
    - Bekräfta att `sowings.batch_number` finns.
    - Bekräfta att `sowing_events` finns med index och RLS-policy för `auth.uid() = user_id`.
    - Bekräfta att funktionen `update_sowing_status_with_event(...)` kan exekveras av `authenticated`.
  3. **Smoke-test i appen**
    - Skapa ny sådd och verifiera status `0`.
    - Kör kedjan `0 -> 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7`.
    - Verifiera att `3 -> 5` blockeras med tydligt fel.
    - Verifiera att `x -> 99` fungerar från aktiva steg men inte från terminala steg.
    - Verifiera att specialdialoger triggas för `0 -> 1`, `-> 6`, `-> 99`.
    - Verifiera att exakt en rad per statusändring skapas i `sowing_events`.
  4. **Övervakning efter deploy**
    - Kontrollera klientloggar för statusuppdateringar och RPC-fel.
    - Kontrollera databasen för oväntade fel i transitions och eventinserts.
  5. **Rollback-plan**
    - Vid blockerande fel: stoppa deploy, återställ databas till snapshot och återdeploya senaste stabila version.
    - Dokumentera incident och vilka migrationssteg som hann tillämpas.

**Föreslagen ordning:** P3-01 → P3-02 → P3-03 → P3-04 → P3-05/P3-06 → P3-07 → P3-08 → P3-09

### Prioritet 4 – Utökat fröinventarie
- Nya fält: inköpsdatum, inköpsställe, grobarhetsprocent
- Varningsindikatorer (gult/rött för utgångsdatum, grått för saldo 0)
- Taggsystem

### Prioritet 5 – Förbättrade såddrekommendationer
- Brådskegrader och färgkodning
- "Snart att så"-sektion på dashboarden
- Direktsådd-logik för `direct_sowing = true`
- Planeringssida (`/planning`)

### Prioritet 6 – Statistik
- Statistiksida (`/statistics`)
- Groningsprocent per art
- Säsongsöversikt

### Prioritet 7 – Notiser (delvis klart)
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
- Radera konto
- Exportera data (JSON-backup)

---

*Specifikation v2.0 – SeedPlan – mars 2026*