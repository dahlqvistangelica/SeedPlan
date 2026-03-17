# SeedPlan 🌱
SeedPlan är en modern trädgårdsapplikation byggd för att hjälpa odlare att hålla koll på sina fröer, planera sina sådder och följa växternas resa från frö till skörd. Genom att integrera användarens lokala frostdatum ger appen personliga rekommendationer för när det är dags att så.

## ✨ Funktioner
- **Interaktiv Dashboard:** Få en snabb överblick över nysådda, grodda och utplanterade växter.
- **Smart Såkalender:** Automatiska förslag på vad som bör sås just nu baserat på ditt inställda frostdatum.
- **Fröbibliotek:** Hantera ditt lager av fröer, sorterade efter växttyp med koll på utgångsdatum och antal.
- **Detaljerad Sådd-spårning:** Följ varje sådds status via en interaktiv tidslinje (från sådd till utplantering).
- **Växtguide:** Sök och filtrera i en omfattande databas över växter med info om härdighet och odlingsråd.
- **PWA-stöd:** Installera appen som en mobilapp för snabb åtkomst i trädgården.

## 🛠 Teknisk Stack
- **Frontend:** Blazor WebAssembly (.NET 8).
- **Backend/API:** Blazor Server & Supabase.
- **Databas & Auth:** Supabase (PostgreSQL & GoTrue).
- **Deployment:** Docker & Railway.

## Kommande funktioner
- **Påminnelser:** Skapa påminnelser om orörda sådder efter ett antal dagar/veckor.
- **Kom ihåg mig:** Funktion för att inte loggas ut automatiskt.
- **Automatisk såddspårning:** Få frågor om sådderna grott/fått karaktärsblad/omskolats/avhärdnings påbörjats efter x antal veckor inaktiva.
- **Fler frö detaljer:** Lägga till inköpsdatum, såråd, inköpsställe på fröer.
- **Ändra sådd:** Möjlighet att ändra antal sådda/variant av sådd. 
- **Samla såråd i databas:** Samla användarnas såråd och publicera i odlingsråd för varje växt.
- **Sådetaljer/noteringar:** Syns i nuläget inte i UI men ska läggas till.

## Bilder
![Dashboard efter inloggning](docs/images/dashboard.png)
![Vyn för sådder](docs/images/sowings.png)