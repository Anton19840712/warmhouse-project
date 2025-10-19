package handlers

import (
	"net/http"
	"strconv"

	"smarthome/services"

	"github.com/gin-gonic/gin"
)

// DeviceHandler handles device-related requests
type DeviceHandler struct {
	DeviceService *services.DeviceService
}

// NewDeviceHandler creates a new DeviceHandler
func NewDeviceHandler(deviceService *services.DeviceService) *DeviceHandler {
	return &DeviceHandler{
		DeviceService: deviceService,
	}
}

// RegisterRoutes registers the device routes
func (h *DeviceHandler) RegisterRoutes(router *gin.RouterGroup) {
	devices := router.Group("/devices")
	{
		devices.GET("", h.GetDevices)
		devices.POST("", h.CreateDevice)
		devices.POST("/:id/command", h.SendCommand)
	}
}

// GetDevices handles GET /api/v1/devices
func (h *DeviceHandler) GetDevices(c *gin.Context) {
	devices, err := h.DeviceService.GetDevices()
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, devices)
}

// CreateDevice handles POST /api/v1/devices
func (h *DeviceHandler) CreateDevice(c *gin.Context) {
	var device services.Device
	if err := c.ShouldBindJSON(&device); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	createdDevice, err := h.DeviceService.CreateDevice(device)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusCreated, createdDevice)
}

// SendCommand handles POST /api/v1/devices/:id/command
func (h *DeviceHandler) SendCommand(c *gin.Context) {
	id, err := strconv.Atoi(c.Param("id"))
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid device ID"})
		return
	}

	var command services.DeviceCommand
	if err := c.ShouldBindJSON(&command); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	if err := h.DeviceService.SendCommand(id, command); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Command sent successfully"})
}
