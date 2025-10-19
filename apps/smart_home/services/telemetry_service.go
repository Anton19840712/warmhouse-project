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
