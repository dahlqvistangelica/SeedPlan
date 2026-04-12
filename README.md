# SeedPlan 🌱

SeedPlan är en PWA för att hantera fröinventering, planera sådder utifrån sista frostdatum och följa såddars utveckling. Appen är byggd med Blazor WebAssembly och Supabase, med svensk UI och mobil-först-design.

README:n speglar nuläget i appen och prioriteringar framåt enligt [SPEC.md](SPEC.md) (april 2026).

## Nuvarande fokus

- Finslipa settings- och profilflödet, främst notiser och tydligare sektionering.
- Utöka rekommendationer på startsidan med bättre prioritering och fler filter.
- Bygga ut fröinventarie, statistik och planering i prioriterad ordning.

## Status just nu

- ✅ Basflöden finns: auth, dashboard, fröinventarie, såddhantering, växtguide, dahlior/knölar och PWA-stöd.
- ✅ Kontofunktioner för e-post/lösenord är implementerade, inklusive validering och felhantering.
- ✅ Såddhantering är utökad med batchnummer, statusflöde, händelselogik och raderingsregler.
- ✅ Push-infrastruktur för orörda sådder finns, inklusive Edge Function för manuell trigger.
- 🟡 Notiser i UI är delvis klara: global toggle finns, men utökad konfiguration saknas.
- 🔨 Framtida fas: planering, statistik, import/export och konto-/datahantering.

## Funktioner i appen (idag)

- Dashboard med rekommenderade arter att så, aktiva sådder och varningar.
- Fröinventarie med lagerhantering, koppling till växtdatabas och **taggar** (egna taggar, filtrering, chips på frökort).
- Såddhantering med statusflöde, batchnummer, historik och misslyckade sådder.
- Växtguide baserad på `plants`-data.
- Dahliabibliotek och eget knölager via separata sidor.
- Supabase Auth (e-post/lösenord) och profilsida med ort och kategorival.
- Push-notiser för orörda sådder (grundinfrastruktur + Edge Function finns).
- PWA-stöd: installerbar app, responsiv mobilvy och grundläggande offline-stöd.

## Viktiga produktbeslut (v2)

- Kontoinställningar i `/profile` fokuserar på profil, ort, e-post och lösenord.
- Odlingszon hanteras i `/settings`.
- Frostdatum och odlingszon hanteras i separata kort/modaler i `/settings`.
- Startsidans förslag filtreras av användaren per växtkategori (`PlantCategory`):
	- `Vegetable`
	- `Flower`
	- `Herb`
	- `Fruit`
- Radera konto, exportera data och import av backup är flyttat till framtida fas (ingår ej i v2).

## Teknisk stack

- Frontend: Blazor WebAssembly (.NET 8)
- Backend/API: Blazor + Supabase
- Databas & Auth: Supabase (PostgreSQL + GoTrue)
	- **Taggar:** Tabell `tags` (egna per användare), kopplingstabeller `seed_tags` och `plant_tags` för många-till-många-relationer.
	- **RLS:** Endast ägaren kan läsa/ändra sina taggar och kopplingar (se migrationsfil 20260410120000_fix_tags_rls.sql).
- Deployment: Docker + Railway

## Prioriterad roadmap (enligt SPEC)

1. Utökat fröinventarie
2. Förbättrade såddrekommendationer
3. Statistik
4. Notiser
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
- Planeringssida som egen komplett vy
- Statistik som egen komplett vy

## Bilder

![Dashboard efter inloggning](docs/images/dashboard.png)
![Vyn för sådder](docs/images/sowings.png)
