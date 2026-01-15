import axios from 'axios';
import {
    CreateOrderRequest,
    OrderAcceptedResponse,
    OrderResponse,
    OrderStatusResponse,
    Product
} from '@/types';

// ════════════════════════════════════════════════════════════════════════
// API Client - Flash Sale Backend
// Baseado em: src/FlashSale.Api/Controllers/*
// ════════════════════════════════════════════════════════════════════════

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

const api = axios.create({
    baseURL: `${API_BASE_URL}/api/v1`,
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000,
});

// ════════════════════════════════════════════════════════════════════════
// Products - GET /api/v1/products
// ════════════════════════════════════════════════════════════════════════

export async function getProducts(): Promise<Product[]> {
    const response = await api.get<Product[]>('/products');
    return response.data;
}

export async function getProduct(id: string): Promise<Product> {
    const response = await api.get<Product>(`/products/${id}`);
    return response.data;
}

// ════════════════════════════════════════════════════════════════════════
// Orders - POST /api/v1/orders (202 Accepted)
// Conforme docs: "Retorna 202 Accepted, cliente recebe orderId e aguarda SignalR"
// ════════════════════════════════════════════════════════════════════════

export async function createOrder(request: CreateOrderRequest): Promise<OrderAcceptedResponse> {
    const response = await api.post<OrderAcceptedResponse>('/orders', request);
    return response.data;
}

// ════════════════════════════════════════════════════════════════════════
// Orders - GET /api/v1/orders/{id}
// ════════════════════════════════════════════════════════════════════════

export async function getOrder(id: string): Promise<OrderResponse> {
    const response = await api.get<OrderResponse>(`/orders/${id}`);
    return response.data;
}

// ════════════════════════════════════════════════════════════════════════
// Orders - GET /api/v1/orders/{id}/status (simplificado)
// ════════════════════════════════════════════════════════════════════════

export async function getOrderStatus(id: string): Promise<OrderStatusResponse> {
    const response = await api.get<OrderStatusResponse>(`/orders/${id}/status`);
    return response.data;
}

// ════════════════════════════════════════════════════════════════════════
// Utilities
// ════════════════════════════════════════════════════════════════════════

/**
 * Gera chave de idempotência única para evitar pedidos duplicados.
 * Formato: order-{timestamp}-{random}
 */
export function generateIdempotencyKey(): string {
    return `order-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
}

/**
 * Extrai parâmetros UTM da URL
 */
export function extractUtmParams(): {
    utmSource?: string;
    utmMedium?: string;
    utmCampaign?: string;
} {
    if (typeof window === 'undefined') return {};

    const params = new URLSearchParams(window.location.search);
    return {
        utmSource: params.get('utm_source') || undefined,
        utmMedium: params.get('utm_medium') || undefined,
        utmCampaign: params.get('utm_campaign') || undefined,
    };
}

export { API_BASE_URL };
