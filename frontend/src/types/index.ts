// ════════════════════════════════════════════════════════════════════════
// DTOs que espelham o backend .NET
// ════════════════════════════════════════════════════════════════════════

export interface Product {
    id: string;
    name: string;
    description?: string;
    price: number;
    stock: number;
    isActive: boolean;
}

export interface OrderItem {
    productId: string;
    quantity: number;
}

export interface CreateOrderRequest {
    customerId: string;
    idempotencyKey: string;
    items: OrderItem[];
    utmSource?: string;
    utmMedium?: string;
    utmCampaign?: string;
}

export interface OrderAcceptedResponse {
    orderId: string;
    status: string;
    message: string;
}

export interface OrderResponse {
    id: string;
    status: 'Pending' | 'Processing' | 'Confirmed' | 'Failed' | 'Cancelled';
    totalAmount: number;
    errorMessage?: string;
    createdAt: string;
    processedAt?: string;
    items: OrderItemResponse[];
}

export interface OrderItemResponse {
    productId: string;
    productName: string;
    quantity: number;
    unitPrice: number;
    subtotal: number;
}

// SignalR Events
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
