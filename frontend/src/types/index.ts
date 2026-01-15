// ════════════════════════════════════════════════════════════════════════
// DTOs que espelham EXATAMENTE o backend .NET
// Baseado em: src/FlashSale.Api/DTOs/*
// ════════════════════════════════════════════════════════════════════════

// ── Products ─────────────────────────────────────────────────────────────

export interface Product {
    id: string;        // Guid no backend
    name: string;
    description?: string;
    price: number;
    stock: number;
    isActive: boolean;
}

// ── Orders ───────────────────────────────────────────────────────────────

export interface OrderItem {
    productId: string; // Guid no backend
    quantity: number;
}

/**
 * Request para POST /api/v1/orders
 * Baseado em: CreateOrderRequest.cs
 */
export interface CreateOrderRequest {
    customerId: string;      // Guid no backend
    idempotencyKey: string;
    items: OrderItem[];
    utmSource?: string;
    utmMedium?: string;
    utmCampaign?: string;
}

/**
 * Response de POST /api/v1/orders (202 Accepted)
 * Baseado em: OrderAcceptedResponse.cs
 */
export interface OrderAcceptedResponse {
    orderId: string;   // Guid no backend
    status: string;
    message: string;
}

/**
 * Response de GET /api/v1/orders/{id}
 * Baseado em: OrderResponse.cs
 */
export interface OrderResponse {
    id: string;
    status: OrderStatus;
    totalAmount: number;
    errorMessage?: string;
    createdAt: string;    // ISO date string
    processedAt?: string; // ISO date string
    items: OrderItemResponse[];
}

export interface OrderItemResponse {
    productId: string;
    productName: string;
    quantity: number;
    unitPrice: number;
    subtotal: number;
}

/**
 * Response de GET /api/v1/orders/{id}/status
 */
export interface OrderStatusResponse {
    orderId: string;
    status: string;
    processedAt?: string;
}

// ── Enums ────────────────────────────────────────────────────────────────

export type OrderStatus = 'Pending' | 'Processing' | 'Confirmed' | 'Failed' | 'Cancelled';

// ── SignalR Events ───────────────────────────────────────────────────────
// Baseado em: SignalRNotificationService.cs

export interface OrderConfirmedEvent {
    orderId: string;
    status: string;
    totalAmount: number;
    timestamp: string;
}

export interface OrderFailedEvent {
    orderId: string;
    status: string;
    reason: string;
    timestamp: string;
}

export interface OrderStatusChangedEvent {
    orderId: string;
    status: string;
}

// ── UTM Parameters ───────────────────────────────────────────────────────

export interface UtmParams {
    utmSource?: string;
    utmMedium?: string;
    utmCampaign?: string;
}
