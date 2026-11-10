// Client for the customer endpoints of TrainBooking.Api.
import { apiFetch } from './http'

export interface Customer {
  id: number
  fullName: string
  email: string
}

export function getCurrentCustomer(customerId: number): Promise<Customer> {
  return apiFetch<Customer>('/api/customers/me', { customerId })
}
