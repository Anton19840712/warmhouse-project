-- Database is already created by POSTGRES_DB env variable
-- and we're already connected to it in docker-entrypoint-initdb.d/

-- Create the sensors table (metadata only, actual readings in TelemetryReadings)
CREATE TABLE IF NOT EXISTS sensors (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    location VARCHAR(100) NOT NULL,
    unit VARCHAR(20),
    status VARCHAR(20) NOT NULL DEFAULT 'inactive',
    last_updated TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS idx_sensors_type ON sensors(type);
CREATE INDEX IF NOT EXISTS idx_sensors_location ON sensors(location);
CREATE INDEX IF NOT EXISTS idx_sensors_status ON sensors(status);

-- Create the telemetry readings table (for Telemetry Service)
CREATE TABLE IF NOT EXISTS "TelemetryReadings" (
    "Id" SERIAL PRIMARY KEY,
    "SensorId" INTEGER NOT NULL,
    "Value" DOUBLE PRECISION NOT NULL,
    "Unit" VARCHAR(20) NOT NULL,
    "Status" VARCHAR(20) NOT NULL,
    "Timestamp" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Location" VARCHAR(100),
    "SensorType" VARCHAR(50),
    CONSTRAINT fk_telemetry_sensor FOREIGN KEY ("SensorId") REFERENCES sensors(id) ON DELETE CASCADE
);

-- Create indexes for telemetry queries
CREATE INDEX IF NOT EXISTS idx_telemetry_sensorid ON "TelemetryReadings"("SensorId");
CREATE INDEX IF NOT EXISTS idx_telemetry_timestamp ON "TelemetryReadings"("Timestamp");

-- Create the devices table (for Device Service)
CREATE TABLE IF NOT EXISTS "Devices" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Location" VARCHAR(100) NOT NULL,
    "Status" VARCHAR(20) NOT NULL,
    "SerialNumber" VARCHAR(50),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastCommandAt" TIMESTAMP WITH TIME ZONE
);

-- Create indexes for device queries
CREATE INDEX IF NOT EXISTS idx_devices_type ON "Devices"("Type");
CREATE INDEX IF NOT EXISTS idx_devices_location ON "Devices"("Location");
CREATE INDEX IF NOT EXISTS idx_devices_status ON "Devices"("Status");

-- Create the device commands table (for Device Service)
CREATE TABLE IF NOT EXISTS "DeviceCommands" (
    "Id" SERIAL PRIMARY KEY,
    "DeviceId" INTEGER NOT NULL,
    "Command" VARCHAR(50) NOT NULL,
    "Parameters" TEXT,
    "Status" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ExecutedAt" TIMESTAMP WITH TIME ZONE,
    FOREIGN KEY ("DeviceId") REFERENCES "Devices"("Id") ON DELETE CASCADE
);

-- Create indexes for device command queries
CREATE INDEX IF NOT EXISTS idx_devicecommands_deviceid ON "DeviceCommands"("DeviceId");
CREATE INDEX IF NOT EXISTS idx_devicecommands_status ON "DeviceCommands"("Status");

-- Insert test sensors
INSERT INTO sensors (name, type, location, unit, status) VALUES
    ('Living Room Temperature', 'temperature', 'Living Room', 'C', 'active'),
    ('Bedroom Humidity', 'humidity', 'Bedroom', '%', 'active'),
    ('Kitchen Temperature', 'temperature', 'Kitchen', 'C', 'active'),
    ('Bathroom Humidity', 'humidity', 'Bathroom', '%', 'inactive'),
    ('Garage Motion Sensor', 'motion', 'Garage', 'boolean', 'active')
ON CONFLICT DO NOTHING;

-- Insert test telemetry readings
INSERT INTO "TelemetryReadings" ("SensorId", "Value", "Unit", "Status", "Timestamp", "Location", "SensorType") VALUES
    (1, 22.5, 'C', 'active', NOW() - INTERVAL '1 hour', 'Living Room', 'temperature'),
    (1, 23.0, 'C', 'active', NOW() - INTERVAL '30 minutes', 'Living Room', 'temperature'),
    (1, 22.8, 'C', 'active', NOW(), 'Living Room', 'temperature'),
    (2, 65.0, '%', 'active', NOW() - INTERVAL '1 hour', 'Bedroom', 'humidity'),
    (2, 67.5, '%', 'active', NOW(), 'Bedroom', 'humidity'),
    (3, 18.5, 'C', 'active', NOW() - INTERVAL '2 hours', 'Kitchen', 'temperature'),
    (3, 19.2, 'C', 'active', NOW() - INTERVAL '1 hour', 'Kitchen', 'temperature'),
    (3, 20.0, 'C', 'active', NOW(), 'Kitchen', 'temperature')
ON CONFLICT DO NOTHING;