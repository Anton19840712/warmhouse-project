package services

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"time"
)

// DeviceService handles communication with Device Management microservice
type DeviceService struct {
	BaseURL    string
	HTTPClient *http.Client
}

// Device represents a device
type Device struct {
	ID            int       `json:"id,omitempty"`
	Name          string    `json:"name"`
	Type          string    `json:"type"`
	Location      string    `json:"location"`
	Status        string    `json:"status"`
	SerialNumber  string    `json:"serialNumber,omitempty"`
	CreatedAt     time.Time `json:"createdAt,omitempty"`
	LastCommandAt time.Time `json:"lastCommandAt,omitempty"`
}

// DeviceCommand represents a command to send to a device
type DeviceCommand struct {
	Command    string `json:"command"`
	Parameters string `json:"parameters,omitempty"`
}

// NewDeviceService creates a new device service
func NewDeviceService(baseURL string) *DeviceService {
	return &DeviceService{
		BaseURL: baseURL,
		HTTPClient: &http.Client{
			Timeout: 10 * time.Second,
		},
	}
}

// GetDevices retrieves all devices
func (s *DeviceService) GetDevices() ([]Device, error) {
	url := fmt.Sprintf("%s/api/devices", s.BaseURL)

	resp, err := s.HTTPClient.Get(url)
	if err != nil {
		return nil, fmt.Errorf("error fetching devices: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	var devices []Device
	if err := json.NewDecoder(resp.Body).Decode(&devices); err != nil {
		return nil, fmt.Errorf("error decoding devices: %w", err)
	}

	return devices, nil
}

// CreateDevice creates a new device
func (s *DeviceService) CreateDevice(device Device) (*Device, error) {
	url := fmt.Sprintf("%s/api/devices", s.BaseURL)

	jsonData, err := json.Marshal(device)
	if err != nil {
		return nil, fmt.Errorf("error marshaling device: %w", err)
	}

	resp, err := s.HTTPClient.Post(url, "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return nil, fmt.Errorf("error creating device: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		return nil, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	var createdDevice Device
	if err := json.NewDecoder(resp.Body).Decode(&createdDevice); err != nil {
		return nil, fmt.Errorf("error decoding device: %w", err)
	}

	return &createdDevice, nil
}

// SendCommand sends a command to a device
func (s *DeviceService) SendCommand(deviceID int, command DeviceCommand) error {
	url := fmt.Sprintf("%s/api/devices/%d/command", s.BaseURL, deviceID)

	jsonData, err := json.Marshal(command)
	if err != nil {
		return fmt.Errorf("error marshaling command: %w", err)
	}

	resp, err := s.HTTPClient.Post(url, "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return fmt.Errorf("error sending command: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	return nil
}
