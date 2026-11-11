import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { StudentBar } from './StudentBar'

describe('StudentBar Component', () => {
  it('renders student registration title and input', () => {
    render(<StudentBar studentId="20081234567890123" onChange={() => {}} />)
    expect(screen.getByText(/Student Birth Registration/i)).toBeDefined()
    const input = screen.getByPlaceholderText(/e\.g\. 20081234567890123/i) as HTMLInputElement
    expect(input.value).toBe('20081234567890123')
  })

  it('displays valid badge when student ID has exactly 17 digits', () => {
    render(<StudentBar studentId="20081234567890123" onChange={() => {}} />)
    expect(screen.getByText(/17 Digits Valid/i)).toBeDefined()
  })

  it('displays invalid character count when ID has fewer than 17 digits', () => {
    render(<StudentBar studentId="12345" onChange={() => {}} />)
    expect(screen.getByText(/5\/17 digits/i)).toBeDefined()
  })

  it('calls onChange when user types numbers', () => {
    const handleChange = vi.fn()
    render(<StudentBar studentId="" onChange={handleChange} />)
    const input = screen.getByPlaceholderText(/e\.g\. 20081234567890123/i)
    fireEvent.change(input, { target: { value: '2008' } })
    expect(handleChange).toHaveBeenCalledWith('2008')
  })
})

