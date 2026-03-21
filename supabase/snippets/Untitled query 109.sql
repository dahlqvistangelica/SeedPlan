DROP VIEW v_user_sowings;

CREATE VIEW v_user_sowings AS
SELECT 
    sw.id,
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
FROM (((sowings sw
    JOIN seeds s ON ((sw.seed_id = s.id)))
    LEFT JOIN plants p ON ((s.plant_id = p.id)))
    LEFT JOIN varieties v ON ((s.variety_id = v.id)));