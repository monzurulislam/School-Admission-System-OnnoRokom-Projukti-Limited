import React, { useState } from 'react'
import { PlusCircle, X, School } from 'lucide-react'
import { api } from '../api/client'

interface CreateClassModalProps {
  isOpen: boolean
  onClose: () => void
  onCreated: () => void
}

export const CreateClassModal: React.FC<CreateClassModalProps> = ({ isOpen, onClose, onCreated }) => {
  const [name, setName] = useState('')
  const [totalSeats, setTotalSeats] = useState<number>(30)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (!isOpen) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim()) {
      setError('Please provide a class name.')
      return
    }
    if (totalSeats < 1) {
      setError('Total seats must be at least 1.')
      return
    }

    try {
      setLoading(true)
      setError(null)
      await api.createClass(name.trim(), totalSeats)
      setName('')
      setTotalSeats(30)
      onCreated()
      onClose()
    } catch (err: any) {
      setError(err.message || 'Failed to create class')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-backdrop">
      <div className="modal-container">
        <div className="modal-header">
          <div className="flex items-center gap-2">
            <School className="w-5 h-5 text-indigo-600" />
            <h3 className="text-base font-semibold text-slate-800">
              Create Admission Class (নতুন শ্রেণি যুক্ত করুন)
            </h3>
          </div>
          <button onClick={onClose} className="modal-close-btn" disabled={loading}>
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-body">
          {error && <div className="alert-error">{error}</div>}

          <div className="form-group">
            <label className="form-label">Class Name & Section (শ্রেণির নাম)</label>
            <input
              type="text"
              required
              placeholder="e.g. Class 6 - Morning Shift (Section A)"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="form-input"
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label className="form-label">Total Seat Capacity (মোট আসন সংখ্যা)</label>
            <input
              type="number"
              min={1}
              required
              value={totalSeats}
              onChange={(e) => setTotalSeats(parseInt(e.target.value) || 1)}
              className="form-input"
              disabled={loading}
            />
          </div>

          <div className="modal-actions">
            <button
              type="button"
              onClick={onClose}
              className="btn-secondary"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn-primary"
              disabled={loading}
            >
              <PlusCircle className="w-4 h-4" />
              {loading ? 'Creating...' : 'Create Class'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

