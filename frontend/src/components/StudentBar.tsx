import React from 'react'
import { UserCheck, AlertCircle, Sparkles, Hash } from 'lucide-react'

interface StudentBarProps {
  studentId: string
  onChange: (id: string) => void
}

export const StudentBar: React.FC<StudentBarProps> = ({ studentId, onChange }) => {
  const isValid = /^\d{17}$/.test(studentId)

  const handlePreset = () => {
    // Generates or sets a valid 17-digit sample ID
    const demoId = '20081234567890123'
    onChange(demoId)
  }

  const handleRandomPreset = () => {
    const random17 = '20' + Math.floor(100000000000000 + Math.random() * 900000000000000).toString()
    onChange(random17)
  }

  return (
    <div className="student-bar-card">
      <div className="student-bar-content">
        <div className="student-bar-label">
          <div className="icon-badge">
            <Hash className="w-5 h-5 text-indigo-600" />
          </div>
          <div>
            <h2 className="text-sm font-semibold text-slate-800">
              Student Birth Registration (জন্ম নিবন্ধন নম্বর)
            </h2>
            <p className="text-xs text-slate-500">
              Required 17-digit number for holds, confirmations, and application tracking
            </p>
          </div>
        </div>

        <div className="student-bar-input-group">
          <div className="relative flex-1">
            <input
              type="text"
              maxLength={17}
              placeholder="e.g. 20081234567890123 (17 digits)"
              value={studentId}
              onChange={(e) => onChange(e.target.value.replace(/\D/g, ''))}
              className={`student-input ${
                isValid
                  ? 'border-emerald-500 ring-2 ring-emerald-100'
                  : studentId.length > 0
                  ? 'border-amber-400 ring-2 ring-amber-50'
                  : 'border-slate-300'
              }`}
            />
            <div className="input-status-badge">
              {isValid ? (
                <span className="badge-valid">
                  <UserCheck className="w-3.5 h-3.5" /> 17 Digits Valid
                </span>
              ) : (
                <span className="badge-counter">
                  {studentId.length}/17
                </span>
              )}
            </div>
          </div>

          <div className="flex gap-2">
            <button
              type="button"
              onClick={handlePreset}
              className="btn-secondary text-xs"
              title="Use sample demo Student ID"
            >
              <Sparkles className="w-3.5 h-3.5 text-indigo-500" /> Demo ID
            </button>
            <button
              type="button"
              onClick={handleRandomPreset}
              className="btn-secondary text-xs"
              title="Generate a new random 17-digit student ID"
            >
              + New Student
            </button>
          </div>
        </div>
      </div>

      {studentId.length > 0 && !isValid && (
        <div className="student-bar-warning">
          <AlertCircle className="w-4 h-4 text-amber-600 shrink-0" />
          <span>Must be exactly 17 numeric digits (বর্তমানে {studentId.length} টি ডিজিট রয়েছে, আরও {17 - studentId.length} টি প্রয়োজন)।</span>
        </div>
      )}
    </div>
  )
}

