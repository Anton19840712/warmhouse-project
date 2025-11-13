package services

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"time"
)

// TelemetryService handles communication with Telemetry microservice
type TelemetryService struct {
	BaseURL    string
	HTTPClient *http.Client
}

// TelemetryReading represents a telemetry reading
type TelemetryReading struct {
	SensorID   int       `json:"sensorId"`
	Value      float64   `json:"value"`
	Unit       string    `json:"unit"`
	Status     string    `json:"status"`
	Timestamp  time.Time `json:"timestamp"`
	Location   string    `json:"location,omitempty"`
	SensorType string    `json:"sensorType,omitempty"`
}

// NewTelemetryService creates a new telemetry service
func NewTelemetryService(baseURL string) *TelemetryService {
	return &TelemetryService{
		BaseURL: baseURL,
		HTTPClient: &http.Client{
			Timeout: 10 * time.Second,
		},
	}
}

// Sensor represents a sensor from Telemetry Service
type Sensor struct {
	ID          int       `json:"id"`
	Name        string    `json:"name"`
	Type        string    `json:"type"`
	Location    string    `json:"location"`
	Unit        string    `json:"unit,omitempty"`
	Status      string    `json:"status"`
	LastUpdated time.Time `json:"lastUpdated"`
	CreatedAt   time.Time `json:"createdAt"`
}

// GetSensors fetches all sensors from Telemetry Service
func (s *TelemetryService) GetSensors() ([]Sensor, error) {
	url := fmt.Sprintf("%s/api/sensors", s.BaseURL)

	resp, err := s.HTTPClient.Get(url)
	if err != nil {
		return nil, fmt.Errorf("error fetching sensors: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	var sensors []Sensor
	if err := json.NewDecoder(resp.Body).Decode(&sensors); err != nil {
		return nil, fmt.Errorf("error decoding response: %w", err)
	}

	return sensors, nil
}

// GetSensorByID fetches a sensor by ID from Telemetry Service
func (s *TelemetryService) GetSensorByID(id int) (*Sensor, error) {
	url := fmt.Sprintf("%s/api/sensors/%d", s.BaseURL, id)

	resp, err := s.HTTPClient.Get(url)
	if err != nil {
		return nil, fmt.Errorf("error fetching sensor: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode == http.StatusNotFound {
		return nil, fmt.Errorf("sensor not found")
	}

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	var sensor Sensor
	if err := json.NewDecoder(resp.Body).Decode(&sensor); err != nil {
		return nil, fmt.Errorf("error decoding response: %w", err)
	}

	return &sensor, nil
}

// SaveReading saves a telemetry reading
func (s *TelemetryService) SaveReading(reading TelemetryReading) error {
	url := fmt.Sprintf("%s/api/telemetry", s.BaseURL)

	jsonData, err := json.Marshal(reading)
	if err != nil {
		return fmt.Errorf("error marshaling reading: %w", err)
	}

	resp, err := s.HTTPClient.Post(url, "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return fmt.Errorf("error saving telemetry: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		return fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	return nil
}
