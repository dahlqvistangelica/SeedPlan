# SeedPlan 🌱

SeedPlan är en PWA för att hantera fröinventering, planera sådder utifrån sista frostdatum och följa såddars utveckling. Appen är byggd med Blazor WebAssembly och Supabase, med svensk UI och mobil-först-design.

README:n speglar nuläget i appen och prioriteringar framåt enligt [SPEC.md](SPEC.md) (april 2026).

## Nuvarande fokus

- Statistiksida (`/statistics`) med groningsprocent och säsongsöversikt.
- Utökad notiskonfiguration i UI.
- Planeringssida (`/planning`) är uppskjuten till höst 2026.

## Status just nu

- ✅ Basflöden finns: auth, dashboard, fröinventarie, såddhantering, växtguide, dahlior/knölar och PWA-stöd.
- ✅ Kontofunktioner för e-post/lösenord är implementerade, inklusive validering och felhantering.
- ✅ Såddhantering är utökad med batchnummer, statusflöde, händelselogik och raderingsregler.
- ✅ Dashboard har ihopfällbara sektioner per sådstatus och frötillgång, med brådskegrader (grön/gul/röd).
- ✅ Fröinventarie utökat med inköpsuppgifter, grobarhetsprocent, taggar och lagervarningar.
- ✅ Trädgårdsplanerare med fri placering, överlappskydd och sådkoppling.
- ✅ Admin: godkännande av dahliasorter och hantering av växtbiblioteket.
- ✅ Push-infrastruktur för orörda sådder finns, inklusive Edge Function för manuell trigger.
- 🟡 Notiser i UI är delvis klara: global toggle finns, men utökad konfiguration saknas.
- 🔨 Framtida fas: statistik, planering, import/export och konto-/datahantering.

## Funktioner i appen (idag)

- **Dashboard** med fyra ihopfällbara sektioner: "Dags att så – har frön", "Dags att så – saknar frön", "Snart dags att så" och "Redan passerat". Brådskegrader (grön/gul/röd punkt) och sortering efter urgency.
- **Fröinventarie** med lagerhantering, inköpsuppgifter, grobarhetsprocent, **taggar** (egna taggar, filtrering, chips på frökort) och visuella lagervarningar.
- **Såddhantering** med statusflöde, batchnummer, historik och misslyckade sådder.
- **Växtguide** baserad på `plants`-data.
- **Trädgårdsplanerare** (`/planner`) med fri placering av växter i odlingsytor, sådkoppling, överlappskydd och drag-and-drop.
- **Dahliabibliotek** och eget knölager via separata sidor.
- Supabase Auth (e-post/lösenord) och profilsida med ort och kategorival.
- Push-notiser för orörda sådder (grundinfrastruktur + Edge Function finns).
- PWA-stöd: installerbar app, responsiv mobilvy och grundläggande offline-stöd.
- **Admin:** `/admin/dahlias` (godkänn/radera inlagda sorter) och `/admin/plants` (lägg till/redigera växtbiblioteket).

## Viktiga produktbeslut (v2)

- Kontoinställningar i `/profile` fokuserar på profil, ort, e-post och lösenord.
- Frostdatum och odlingszon hanteras i separata kort/modaler i `/settings`.
- Startsidans förslag filtreras av användaren per växtkategori (`PlantCategory`): `Vegetable`, `Flower`, `Herb`, `Fruit`.
- Radera konto, exportera data och import av backup är flyttat till framtida fas (ingår ej i v2).
- Planeringssida (`/planning`) uppskjuten till höst 2026.

## Teknisk stack

- Frontend: Blazor WebAssembly (.NET 8)
- Backend/API: Direkt mot Supabase via Postgrest SDK (ingen mellanliggande API-layer)
- Databas & Auth: Supabase (PostgreSQL + GoTrue), RLS per användare
  - **Taggar:** `tags` (egna per användare), `seed_tags` och `plant_tags` för många-till-många
  - **Planerare:** `cultivation_areas` och `planted_crops` med centroid-koordinater i cm
- Deployment: Docker + Railway

## Prioriterad roadmap (enligt SPEC)

1. ✅ Utökat fröinventarie
2. ✅ Förbättrade såddrekommendationer (planeringssida uppskjuten till höst)
3. Statistik (`/statistics`)
4. Notiser (utökad konfiguration)
5. Konto- och datahantering

## Ingår ej i v2 (framtida fas)

- Radera konto
- Exportera data (JSON-backup)
- Import av backup-data
- Kalendervy
- Adaptiva rekommendationer
- Bildlogg per sådd
- Export till PDF/CSV
- Delning av data mellan användare
- Väderprognos-integration
- Planeringssida (`/planning`) – höst 2026

## Bilder

![Dashboard efter inloggning](docs/images/dashboard.png)
![Vyn för sådder](docs/images/sowings.png)
