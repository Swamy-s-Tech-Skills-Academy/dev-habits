-- Insert 10 sample habits with essential fields
INSERT INTO dev_habit.habits
(id, name, description, type, frequency_type, frequency_times_per_period, target_value, target_unit, status, created_at_utc,is_archived)
VALUES
('h_01HNK4V8J5T6MW8X9Y0Z1A2B3C', 'Daily Meditation', 'Morning mindfulness practice', 2, 1, 1, 15, 'minutes', 1, '2025-02-03T08:00:00Z',false),
('h_01HNK4V8P7Q8RX9Y0Z1A2B3C4D', 'Read Book', '30 pages per day', 2, 1, 1, 30, 'pages', 1, '2025-02-03T09:00:00Z',false),
('h_01HNK4V8T6U7VW8X9Y0Z1A2B3C', 'Exercise', 'Daily workout session', 2, 1, 1, 45, 'minutes', 1, '2025-02-03T10:00:00Z',false),
('h_01HNK4V8X9Y0Z1A2B3C4D5E6F7', 'Drink Water', 'Stay hydrated', 2, 1, 1, 2000, 'milliliters', 1, '2025-02-03T11:00:00Z',false),
('h_01HNK4V91A2B3C4D5E6F7G8H9I', 'Journal', 'Daily reflection', 1, 1, 1, 0, '', 1, '2025-02-03T12:00:00Z',false),
('h_01HNK4V95B6C7D8E9F0G1H2I3J', 'Practice Guitar', 'Musical practice', 2, 1, 1, 20, 'minutes', 1, '2025-02-03T13:00:00Z',false),
('h_01HNK4V99C0D1E2F3G4H5I6J7K', 'Take Vitamins', 'Daily supplements', 1, 1, 1, 0, '', 1, '2025-02-03T14:00:00Z',false),
('h_01HNK4V9D3E4F5G6H7I8J9K0L1', 'Study Spanish', 'Language learning', 2, 1, 1, 30, 'minutes', 1, '2025-02-03T15:00:00Z',false),
('h_01HNK4V9H7I8J9K0L1M2N3O4P5', 'Walk Steps', 'Daily movement goal', 2, 1, 1, 10000, 'steps', 1, '2025-02-03T16:00:00Z',false),
('h_01HNK4V9H7I8J9K0L1M2N3O4P6', 'Meal Planning', 'Plan meals for the week', 2, 1, 1, 1, '', 1, '2025-02-03T16:00:00Z',false);