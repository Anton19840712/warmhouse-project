package handlers

import (
	"fmt"
	"log"
	"net/http"
	"strconv"

	"smarthome/services"

	"github.com/gin-gonic/gin"
)

// SensorHandler handles sensor-related requests
type SensorHandler struct {
	TemperatureService *services.TemperatureService
	TelemetryService   *services.TelemetryService
}

// NewSensorHandler creates a new SensorHandler
func NewSensorHandler(temperatureService *services.TemperatureService, telemetryService *services.TelemetryService) *SensorHandler {
	return &SensorHandler{
		TemperatureService: temperatureService,
		TelemetryService:   telemetryService,
	}
}

// RegisterRoutes registers the sensor routes
func (h *SensorHandler) RegisterRoutes(router *gin.RouterGroup) {
	sensors := router.Group("/sensors")
	{
		sensors.GET("", h.GetSensors)
		sensors.GET("/:id", h.GetSensorByID)
		// Create/Update/Delete sensors should be done directly via TelemetryService API
		sensors.GET("/temperature/:location", h.GetTemperatureByLocation)
	}
}

// GetSensors handles GET /api/v1/sensors
func (h *SensorHandler) GetSensors(c *gin.Context) {
	// Proxy to Telemetry Service
	sensors, err := h.TelemetryService.GetSensors()
	if err != nil {
		log.Printf("Failed to get sensors from Telemetry Service: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch sensors"})
		return
	}

	c.JSON(http.StatusOK, sensors)
}

// GetSensorByID handles GET /api/v1/sensors/:id
func (h *SensorHandler) GetSensorByID(c *gin.Context) {
	id, err := strconv.Atoi(c.Param("id"))
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid sensor ID"})
		return
	}

	// Proxy to Telemetry Service
	sensor, err := h.TelemetryService.GetSensorByID(id)
	if err != nil {
		log.Printf("Failed to get sensor %d from Telemetry Service: %v", id, err)
		c.JSON(http.StatusNotFound, gin.H{"error": "Sensor not found"})
		return
	}

	c.JSON(http.StatusOK, sensor)
}

// GetTemperatureByLocation handles GET /api/v1/sensors/temperature/:location
func (h *SensorHandler) GetTemperatureByLocation(c *gin.Context) {
	location := c.Param("location")
	if location == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Location is required"})
		return
	}

	// Fetch temperature data from the external API
	tempData, err := h.TemperatureService.GetTemperature(location)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{
			"error": fmt.Sprintf("Failed to fetch temperature data: %v", err),
		})
		return
	}

	// Save telemetry reading to Telemetry Service
	sensorID, _ := strconv.Atoi(tempData.SensorID)
	telemetryReading := services.TelemetryReading{
		SensorID:   sensorID,
		Value:      tempData.Value,
		Unit:       tempData.Unit,
		Status:     tempData.Status,
		Timestamp:  tempData.Timestamp,
		Location:   tempData.Location,
		SensorType: tempData.SensorType,
	}

	if err := h.TelemetryService.SaveReading(telemetryReading); err != nil {
		log.Printf("Failed to save telemetry reading: %v", err)
		// Don't fail the request if telemetry save fails
	} else {
		log.Printf("Telemetry reading saved for sensor %d", sensorID)
	}

	// Return the temperature data
	c.JSON(http.StatusOK, gin.H{
		"location":    tempData.Location,
		"value":       tempData.Value,
		"unit":        tempData.Unit,
		"status":      tempData.Status,
		"timestamp":   tempData.Timestamp,
		"description": tempData.Description,
	})
}
