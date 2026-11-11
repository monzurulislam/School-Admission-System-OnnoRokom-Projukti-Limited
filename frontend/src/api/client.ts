export interface ClassItem {
  id: string
  name: string
  totalSeats: number
  heldSeats: number
  confirmedSeats: number
  availableSeats: number
}

export interface HoldItem {
  holdId: string
  classId: string
  studentId?: string
  status: 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED'
  expiresAt: string
}

export interface StudentHoldItem {
  holdId: string
  classId: string
  className: string
  status: 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED'
  expiresAt: string
}

const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE}${path}`
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (response.status === 204) {
    return {} as T
  }

  const text = await response.text()
  let data: any = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!response.ok) {
    const errorMsg = data?.error || (typeof data === 'string' ? data : `HTTP ${response.status}`)
    throw new Error(errorMsg)
  }

  return data as T
}

export const api = {
  // Classes
  getClasses: () => request<ClassItem[]>('/classes'),
  getClassById: (classId: string) => request<ClassItem>(`/classes/${classId}`),
  createClass: (name: string, totalSeats: number) =>
    request<ClassItem>('/classes', {
      method: 'POST',
      body: JSON.stringify({ name, totalSeats }),
    }),

  // Holds
  createHold: (classId: string, studentId: string) =>
    request<HoldItem>(`/classes/${classId}/holds`, {
      method: 'POST',
      headers: { 'X-Student-Id': studentId },
    }),

  getHold: (holdId: string) => request<HoldItem>(`/holds/${holdId}`),

  confirmHold: (holdId: string, studentId: string) =>
    request<{ holdId: string; status: string }>(`/holds/${holdId}/confirm`, {
      method: 'POST',
      headers: { 'X-Student-Id': studentId },
    }),

  cancelHold: (holdId: string, studentId: string) =>
    request<void>(`/holds/${holdId}`, {
      method: 'DELETE',
      headers: { 'X-Student-Id': studentId },
    }),

  getMyHolds: (studentId: string) =>
    request<StudentHoldItem[]>('/me/holds', {
      headers: { 'X-Student-Id': studentId },
    }),
}

