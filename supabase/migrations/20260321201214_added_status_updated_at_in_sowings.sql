drop view if exists "public"."v_user_sowings";

alter table "public"."sowings" add column "status_updated_at" timestamp with time zone default now();

create or replace view "public"."v_user_sowings" as  SELECT sw.id,
    sw.user_id,
    sw.sown_date,
    sw.status,
    sw.notes,
    sw.quantity,
    sw.seed_id,
    sw.status_updated_at,
    s.name AS seed_name,
    p.plant_name,
    v.variety_name,
    p.sowing_depth_mm,
    p.is_light_germinating,
    p.requires_topping
   FROM (((public.sowings sw
     JOIN public.seeds s ON ((sw.seed_id = s.id)))
     LEFT JOIN public.plants p ON ((s.plant_id = p.id)))
     LEFT JOIN public.varieties v ON ((s.variety_id = v.id)));



