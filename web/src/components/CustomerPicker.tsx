import { useEffect, useState } from 'react'
import { getCurrentCustomer, type Customer } from '../api/customers'

interface CustomerPickerProps {
  customerIds: number[]
  customerId: number
  onChange: (customerId: number) => void
}

/**
 * Chooses which customer the app acts as. Stand-in for real sign-in: the id is sent as X-Customer-Id.
 */
export function CustomerPicker({ customerIds, customerId, onChange }: CustomerPickerProps) {
  const [customer, setCustomer] = useState<Customer | null>(null)

  useEffect(() => {
    let cancelled = false
    getCurrentCustomer(customerId)
      .then((c) => {
        if (!cancelled) setCustomer(c)
      })
      .catch(() => {
        if (!cancelled) setCustomer(null)
      })
    return () => {
      cancelled = true
    }
  }, [customerId])

  return (
    <label className="customer-picker">
      <span className="customer-name">{customer ? customer.fullName : 'Unknown customer'}</span>
      <select value={customerId} onChange={(e) => onChange(Number(e.target.value))} aria-label="Act as customer">
        {customerIds.map((id) => (
          <option key={id} value={id}>
            Customer #{id}
          </option>
        ))}
      </select>
    </label>
  )
}
