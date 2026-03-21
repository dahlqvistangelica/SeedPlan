UPDATE sowings 
SET status_updated_at = NOW() - INTERVAL '25 days'
WHERE id = 1; -- byt till ett riktigt id