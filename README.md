# SeedPlan 🌱

SeedPlan är en PWA för att hantera fröinventering, planera sådder utifrån sista frostdatum och följa såddars utveckling. Appen är byggd med Blazor WebAssembly och Supabase, med svensk UI och mobil-först-design.

README:n speglar nuläget i appen och prioriteringar framåt enligt [SPEC.md](SPEC.md) (mars 2026).

## Nuvarande fokus

- Finslipa settings- och profilflödet (UX och tydligare gruppering av inställningar).
- Utöka rekommendationer på startsidan med bättre prioritering och fler filter.
- Fortsätta bygga ut fröinventarie och statistik i prioriterad ordning.

## Status just nu

- ✅ Basflöden finns: auth, dashboard, fröinventarie, såddhantering, växtguide, PWA-stöd.
- ✅ Kontofunktioner för e-post/lösenord är implementerade (inklusive validering och felhantering).
- ✅ Såddhantering är utökad med batchnummer, fler statusar och händelselogik.
- ✅ Push-infrastruktur för orörda såddar finns (inklusive Edge Function för manuell trigger).
- 🟡 Notiser i UI är delvis klara: global toggle finns, men utökad konfiguration saknas.
- 🔨 Framtida fas: kontoradering, JSON-backup/export, planering och statistik som egna sidor.

## Funktioner i appen (idag)

- Dashboard med rekommenderade arter att så, aktiva såddar och varningar.
- Fröinventarie med lagerhantering och koppling till växtdatabas.
- Såddhantering med statusflöde, batchnummer och översikter.
- Växtguide baserad på `plants`-data.
- Supabase Auth (e-post/lösenord) och profilsida.
- Kategorival i profil för vilka växttyper som ska visas i såförslag.
- Push-notiser för orörda såddar (grundinfrastruktur + Edge Function finns).
- PWA-stöd: installerbar app, responsiv mobilvy och grundläggande offline-stöd.

## Viktiga produktbeslut (v2)

- Kontoinställningar i `/profile` fokuserar på profil, e-post och lösenord.
- Odlingszon hanteras i `/settings`.
- Frostdatum och odlingszon hanteras i separata kort/modaler i `/settings`.
- Startsidans förslag ska kunna filtreras av användaren per växtkategori (`PlantCategory`):
	- `Vegetable`
	- `Flower`
	- `Herb`
	- `Fruit`
- Radera konto och exportera data är flyttat till framtida fas (ingår ej i v2).

## Teknisk stack

- Frontend: Blazor WebAssembly (.NET 8)
- Backend/API: Blazor + Supabase
- Databas & Auth: Supabase (PostgreSQL + GoTrue)
- Deployment: Docker + Railway

## Prioriterad roadmap (enligt SPEC)

1. Utökat fröinventarie
2. Förbättrade såddrekommendationer
3. Statistik och planering som egna sidor
4. Notiser (utökad konfiguration)
5. Konto och datahantering i framtida fas (radera konto/export)

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

## Bilder

![Dashboard efter inloggning](docs/images/dashboard.png)
![Vyn för sådder](docs/images/sowings.png)
