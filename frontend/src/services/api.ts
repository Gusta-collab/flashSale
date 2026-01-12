import axios from 'axios';
import { CreateOrderRequest, OrderAcceptedResponse, OrderResponse, Product } from '@/types';

// ════════════════════════════════════════════════════════════════════════
// API Client - Flash Sale Backend
// ════════════════════════════════════════════════════════════════════════

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

const api = axios.create({
    baseURL: `${API_BASE_URL}/api/v1`,
    headers: {
        'Content-Type': 'application/json',
    },
});

// ════════════════════════════════════════════════════════════════════════
// Products
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
// Orders
// ════════════════════════════════════════════════════════════════════════

export async function createOrder(request: CreateOrderRequest): Promise<OrderAcceptedResponse> {
    const response = await api.post<OrderAcceptedResponse>('/orders', request);
    return response.data;
}

export async function getOrder(id: string): Promise<OrderResponse> {
    const response = await api.get<OrderResponse>(`/orders/${id}`);
    return response.data;
}

export async function getOrderStatus(id: string): Promise<{ orderId: string; status: string; processedAt?: string }> {
    const response = await api.get(`/orders/${id}/status`);
    return response.data;
}

// ════════════════════════════════════════════════════════════════════════
// Utilities
// ════════════════════════════════════════════════════════════════════════

export function generateIdempotencyKey(): string {
    return `order-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
}

export { API_BASE_URL };
