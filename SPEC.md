# SeedPlan – Specifikation v2.2

> Baserad på befintlig app (Blazor WebAssembly + Supabase) – april 2026
> Odlingsplatser är borttagna. Speccen speglar nuvarande app och visar tydligt vad som redan finns och vad som återstår.

---

## 1. Översikt

SeedPlan är en PWA för att hantera fröinventering, planera sådder baserat på sista frostdatum, följa såddars utveckling steg för steg, hålla koll på dahlior/knölar och ge en tydlig profil- och inställningsyta. All data lagras i Supabase med individuella användarkonton.

Nuvarande app innehåller dashboard, fröer, sådder, växtguide, dahlior, knölar, profil och inställningar, samt två tydliga applägen:
- **SeedPlan-läge** (fröer/sådder/guide)
- **DahliaBox-läge** (dahliaöversikt/egna knölar/varianter)

### Befintlig stack (ändras ej)

| Egenskap | Val |
|---|---|
| Typ | PWA – Blazor WebAssembly (.NET 8) |
| Databas & Auth | Supabase (PostgreSQL + GoTrue) |
| Deployment | Docker + Railway |
| Språk | Svenska UI |

### PWA-krav (befintliga + utökade)
- Installerbar på hemskärm via `manifest.webmanifest` ✅
- Offline-läsning av inventarie och aktiva sådder via Service Worker ✅ (grundläggande)
- Push-notiser via Web Push API 🟡 (grundinfrastruktur finns, men konfigurationen är fortfarande delvis manuell)
- Responsiv design, primärt mobilvy ✅

### 1.1 Nyligen levererat i UI och funktioner (release 1.5.1)
- Appversion uppdaterad till **1.5.1** i klientens versionskontroll, settingsvisning och publik appsettings.
- Växling mellan **SeedPlan** och **DahliaBox** sker via loggor i headern.
- Aktivt appläge sparas i `localStorage` (`appMode`) och återställs vid sidladdning.
- `BottomNav` är lägesstyrd:
  - SeedPlan-läge: Översikt, Fröer, Sådder, Växtguide, Inställningar
  - DahliaBox-läge: Översikt, Egna knölar, Varianter, Inställningar
- DahliaBox har en egen startsida (`/dahliabox-home`) med:
  - nyckeltal (antal sorter, antal knölar, favorittyp)
  - säsongstidslinje (förodling och utplantering utifrån användarens sista frostdatum)
- Dahlia-modaler använder återanvändbar funktionskomponent för betyg (`StarRating`) med klickbar 1-5-stjärnlogik.
- Dahlia-sökning i `AddDahliaModal` är nu fullt asynkron i UI-event (`@oninput` + `await`), i stället för blockerande `.Result`.
- Dahlia-sökning begränsas till toppträffar (limit) och sorteras alfabetiskt för snabb respons i dropdown.

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

### 3.0 Nuvarande läge (april 2026)
- `/settings` används som inställningsöversikt med frostdatum, plats/växtzon, aviseringar, profilgenväg, info och adminlänk för admins.
- `/profile` innehåller profil- och kontouppgifter: namn, ort, kategorival för startsidans såförslag samt e-post- och lösenordsbyte.
- Ort finns redan i modellen och används för vädervarningar via geokodade koordinater (`city`, `latitude`, `longitude`).
- Frostdatum och odlingszon hanteras som separata kort/modaler i `/settings`.
- Odlingszon visas inte i kontoinställningar i `/profile`.
- `preferred_plant_categories` är en `int[]` i modellagret och styr vilka växtkategorier som visas i rekommendationerna.
- Demo-kontot har avstängda kontoflöden för e-post och lösenord men kan fortfarande uppdatera profiluppgifter.

### Befintliga fält (ändras ej)
- Namn
- Ort
- Sista frostdatum
- Odlingszon (1–8)
- Valda växtkategorier

### Nuvarande beteende
- Ort används både för visning och som input till vädervarningar.
- Om inga kategorier är valda visas ett tomt tillstånd i dashboarden med uppmaning att välja minst en kategori.
- Avbockade kategorier tas bort från `preferred_plant_categories` i stället för att bara döljas i UI-state.

### Struktur

Nuvarande struktur:
```
Inställningsflöde
├── Översikt (`/settings`)
│   ├── Frostdatum (egen modal)
│   ├── Plats / växtzon (egen modal)
│   ├── Aviseringar
│   ├── Profil (länk till `/profile`)
│   ├── Info
│   └── Admin (endast administratörer)
└── Profil (`/profile`)
  ├── Profiluppgifter (namn, ort + kategorival)
  └── Kontouppgifter (ändra e-post, ändra lösenord)
```

Målstruktur (kommande iteration):
- Flytta/sektionera notisinställningar tydligare när `notification_settings` införs.
- Behåll settings som översikt och profil som platsen för personliga uppgifter och kontoåtgärder.

---

## 4. Fröinventariet (utökat)

### 4.1 Befintliga fält (ändras ej)
- Art (koppling till växtdatabas)
- Variant/sort (fritext)
- Antal frön
- Utgångsdatum
- Anteckningar

Nuvarande UI i `AddSeedModal` och `EditSeedModal` hanterar just dessa fält. När en ny sort anges för en vald art skapas sorten automatiskt om den inte redan finns.

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
- Lagret får aldrig gå under noll – sådd blockeras med felmeddelande ✅
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

### 5.4 Bildhantering för växter
- Bildfunktionen som idag används för dahlior ska även finnas för övriga växter med samma uppladdnings- och beskärningsstruktur.
- Bilden laddas upp i användarens egen mapp i Supabase Storage.
- Om växten har en vald variant sparas URL:en på varianten först.
- Om ingen variant finns eller ingen variant är vald sparas URL:en direkt på plantan.
- När en variant får eller uppdaterar sin bild ska plantan också spegla samma URL som fallback.
- Vid visning ska variantens bild prioriteras före plantans bild.
- Bilden ska kunna visas i växtguide, plantdetaljvy och övriga växtkort där bild stöds.
- Egna växter ska följa samma bildstruktur som inbyggda växter.

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

**Dashboardvyn (befintlig):**
- "Bör sås nu" – aktiva fönster, sorterade på brådska, med färgkodning ✅
- "Redan passerat" – befintlig accordion-sektion ✅

**Planerat i nästa iteration:**
- "Snart att så" – ny sektion för kommande 14 dagar

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

### 7.1 Nuvarande statusflöde

Statuskoderna som används i appen är:
```
0 = Sådd
1 = Groddning
2 = Karaktärsblad
3 = Omskolning
4 = Avhärdning
5 = Utplanterad
6 = Skörd
7 = Avslutad
99 = Misslyckad
```

Detaljvyn för en sådd visar en klickbar tidslinje för steg 0-7, och misslyckad status kan sättas separat från åtgärdsknapparna.

### 7.2 Omgångsnummer
- Flera aktiva såddar av samma frö/art tillåts
- Omgångsnummer föreslås automatiskt utifrån nästa lediga nummer för valt frö
- Visas i UI som exempelvis "Omgång 1", "Omgång 2"
- `batch_number` finns redan på `sowings`-tabellen

### 7.3 Händelselogg per sådd
Varje statusövergång loggas i tabellen `sowing_events`:

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

**Vad som loggas automatiskt och manuellt:**
- Statusövergång till 1 (Groddning): frågar om antal grodda plantor
- Statusövergång till 6 (Skörd): frågar om skördevikt eller antal skördade
- Statusövergång till 99 (Misslyckad): frågar om orsak, fritext är valfritt
- Övriga övergångar: loggas med datum och eventuella anteckningar

### 7.4 Radera en sådd
- Bekräftelsedialog finns
- Status < Groddning (0): frön återförs till lagret automatiskt
- Status ≥ Groddning (1+): frön återförs inte, men information visas för användaren
- Händelseloggar raderas via cascade delete

### 7.5 Misslyckad sådd
- Knapp för att markera som misslyckad finns i detaljvyn
- Dialogen öppnar ett valfritt fritextfält för orsak
- Frön som dragits från lagret återförs inte
- Misslyckade såddar visas fortfarande i listor och kan filtreras fram

---

## 8. Notissystem

### 8.1 Vad som redan finns ✅

**Klientsidan (`NotificationService.cs` + `notifications.js`):**
- Webbläsaren kan be om notistillstånd vid behov
- Push-prenumeration kan skapas och sparas i Supabase-tabellen `push_subscriptions`
- Home-sidan försöker registrera push för inloggade användare som inte är demo-användare
- Inställningar för aviseringar finns som global toggle i `/settings` och sparas lokalt i webbläsaren
- Varningskort för orörda sådder visas på dashboarden

**Serversidan:**
- Edge Function `send-sowing-reminders` finns för manuell trigger av push-flödet
- Funktionens ansvar är att läsa aktiva såddar, avgöra om de är för gamla och skicka push-notiser

**Databasstruktur:**
- `push_subscriptions` finns redan

### 8.2 Vad som saknas och ska byggas

**Automatisk daglig trigger:**
- Edge Function finns men saknar schemalagd körning via cron eller Supabase Scheduled Functions

**Notiser för såddrekommendationer:**
- Kommande såddfönster notifieras inte ännu
- Triggerlogik ska baseras på rekommenderat datum per användare och konfigureras i UI senare

**Inställningsgränssnitt:**
- 🟡 Global av/på för aviseringar finns redan i `/settings`
- 🔨 Detaljerad notiskonfiguration saknas
- 🔨 Notisinställningar behöver samlas tydligare enligt målstrukturen i sektion 3

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
| Orörd sådd | ✅ finns | Varningslogik på dashboarden + Edge Function | "Har [Art] grott? Den har stått orörd i X dagar." |
| Kommande såddfönster | 🔨 ska byggas | Edge Function-utökning | "Dags snart att så [Art]! Rekommenderat datum: [datum]." |
| Notiskonfiguration i UI | 🟡 delvis | Global toggle finns i `/settings`; konfiguration saknas | Inställningsflödet (`/settings` + `/profile`) |

---

## 9. Statistik & Analys – planerad funktion

Status (april 2026): 🔨 ingen dedikerad sida ännu (`/statistics` saknas).

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
ALTER TABLE user_profiles ADD COLUMN city text;
ALTER TABLE user_profiles ADD COLUMN latitude double precision;
ALTER TABLE user_profiles ADD COLUMN longitude double precision;
ALTER TABLE user_profiles ADD COLUMN preferred_plant_categories int[] DEFAULT '{0,1,2,3}';
-- OBS: push_subscriptions sparas INTE i user_profiles utan i separat push_subscriptions-tabell (finns redan)

-- Lägg till fält på plants (inbyggd växtdatabas)
ALTER TABLE plants ADD COLUMN germination_days_min int;
ALTER TABLE plants ADD COLUMN germination_days_max int;
ALTER TABLE plants ADD COLUMN days_to_harvest int;
ALTER TABLE plants ADD COLUMN sowing_notes text;
ALTER TABLE plants ADD COLUMN photo_url text;

-- Lägg till fält på varieties
ALTER TABLE varieties ADD COLUMN photo_url text;

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
  photo_url text,
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

-- Notiskonfiguration (framtida)
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
📱 Header-lägeväljare:
├── SeedPlan-logo   -> växlar till SeedPlan-läge
└── Dahlia-logo     -> växlar till DahliaBox-läge (inloggad användare)

📱 Bottom navigation – SeedPlan-läge:
├── 🏠 Översikt        /              – Sårekommendationer + aktiva sådder + varningar
├── 🌱 Fröer           /seeds         – Frölagret
├── 🌿 Såddar         /sowings       – Aktiva och avslutade såddar
├── 📖 Växtguide       /guide         – Växtguide
└── ⚙️ Inställn.      /settings      – Inställningsöversikt + länk till profil/konto

📱 Bottom navigation – DahliaBox-läge:
├── 🏠 Översikt        /dahliabox-home – DahliaBox dashboard
├── 🌱 Egna knölar     /tubers         – Användarens knöllager
├── 🌸 Varianter       /dahlias        – Dahliabibliotek + filtrering/sök
└── ⚙️ Inställn.      /settings       – Delad inställningsyta
```

Notering: Önskelista-fliken är tillfälligt utkommenterad i navigeringen och ingår inte i aktivt flöde.

Övriga sidor som är planerade men inte implementerade i navigationen ännu:
- `/planning` – fullständig såddlista för säsongen
- `/statistics` – groningsprocent och säsongsöversikt

---

## 12. Affärsregler (uppdaterade)

1. Lagret får aldrig gå under noll – sådd blockeras med felmeddelande
2. Frön återförs till lagret om sådd raderas **innan** status Groddning (status < 1)
3. Frön återförs **inte** om sådd raderas vid status ≥ Groddning – informationstext visas
4. Sista frostdatum är globalt per konto
5. Ort används för vädervarningar, men odlingszon och sista frostdatum styr planeringen
6. Flera såddar av samma art är tillåtet – identifieras med omgångsnummer
7. Statistik beräknas alltid från faktiska loggade händelser, aldrig uppskattningar
8. All användardata är strikt isolerad via Supabase RLS
9. Kategorifilter på startsidan styr vilka växtkategorier som visas i rekommendationssektionerna
10. Demo-kontot får inte ändra e-post eller lösenord

---

## 13. Byggas i denna prioritetsordning

### Prioritet 1 – Buildlista (nästa releasekandidat)
- Lägg till visuell loading-state i dahlia-sökdropdown när asynkron sökning pågår (`isSearching`).
- Lägg till enkel debounce/cancellation i dahlia-sökning för att minska överlappande requests vid snabb inmatning.
- Säkerställ konsekvent fallback när sökning ger 0 träffar (tomt tillstånd + CTA för "Skapa ny sort").
- QA av lägesväxling SeedPlan/DahliaBox (persistens i `localStorage`, korrekt redirect, korrekt active state i UI).
- QA av mode-specifik bottomnav på mobil och desktop (inga brutna länkar mellan lägen).
- Verifiera versionskedjan 1.5.1 (`MainLayout`, `Settings`, `wwwroot/appsettings.json`) i cache-scenario.
- Lägg till/uppdatera tester för dahlia-sökflödet (asynkront input-event, resultatlista, val av träff).
- Lägg till/uppdatera tester för `StarRating`-komponenten (toggle av samma stjärna => `null`).

### Prioritet 1 – Utökat fröinventarie
- Inköpsdatum, inköpsställe och grobarhetsprocent
- Taggsystem och filtrering på tagg
- Visuella lagervarningar för utgångsdatum och nollsaldo

### Prioritet 2 – Förbättrade såddrekommendationer
- "Snart att så"-sektion på dashboarden
- Planeringssida (`/planning`)
- Tydligare brådskegrader och färgkodning i alla rekommendationsvyer

### Prioritet 3 – Statistik
- Statistiksida (`/statistics`)
- Groningsprocent per art
- Säsongsöversikt baserad på `sowing_events`

### Prioritet 4 – Notiser
- Automatisk daglig cron-trigger i Supabase
- Utökade påminnelser för kommande såddfönster
- Tydligare notiskonfiguration i UI via `notification_settings`

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

*Specifikation v2.2 – SeedPlan – april 2026*