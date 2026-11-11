import { useState, useEffect, useCallback, useRef } from 'react'
import { GraduationCap, Plus, RefreshCw, AlertCircle, CheckCircle, ShieldCheck } from 'lucide-react'
import { api } from './api/client'
import type { ClassItem, StudentHoldItem } from './api/client'
import { StudentBar } from './components/StudentBar'
import { ClassCard } from './components/ClassCard'
import { CreateClassModal } from './components/CreateClassModal'
import { MyApplications } from './components/MyApplications'
import './App.css'

export function App() {
  const [studentId, setStudentId] = useState<string>(() => {
    return localStorage.getItem('school_admission_student_id') || '20081234567890123'
  })

  const [classes, setClasses] = useState<ClassItem[]>([])
  const [myHolds, setMyHolds] = useState<StudentHoldItem[]>([])
  const [activeTab, setActiveTab] = useState<'classes' | 'applications'>('classes')
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isApplyingClassId, setIsApplyingClassId] = useState<string | null>(null)
  const [errorToast, setErrorToast] = useState<string | null>(null)
  const [successToast, setSuccessToast] = useState<string | null>(null)
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date())
  const [isRefreshing, setIsRefreshing] = useState(false)

  const pollingRef = useRef<number | null>(null)

  // Persist student ID
  const handleStudentIdChange = (id: string) => {
    setStudentId(id)
    localStorage.setItem('school_admission_student_id', id)
  }

  // Fetch classes and student holds
  const fetchClassesAndHolds = useCallback(async (showIndicator = false) => {
    if (showIndicator) setIsRefreshing(true)

    try {
      const classData = await api.getClasses()
      setClasses(classData)
      setLastUpdated(new Date())

      if (/^\d{17}$/.test(studentId)) {
        try {
          const holdData = await api.getMyHolds(studentId)
          setMyHolds(holdData)
        } catch {
          // ignore error if student has no holds yet
        }
      } else {
        setMyHolds([])
      }
    } catch (err: any) {
      console.error('Failed to fetch data:', err)
    } finally {
      if (showIndicator) setIsRefreshing(false)
    }
  }, [studentId])

  // Setup 3-second live auto-polling
  useEffect(() => {
    fetchClassesAndHolds(true)

    pollingRef.current = window.setInterval(() => {
      fetchClassesAndHolds(false)
    }, 3000)

    return () => {
      if (pollingRef.current) clearInterval(pollingRef.current)
    }
  }, [fetchClassesAndHolds])

  // Apply / Hold a seat
  const handleApply = async (classId: string) => {
    if (!/^\d{17}$/.test(studentId)) {
      setErrorToast('Please enter a valid 17-digit student registration number.')
      return
    }

    try {
      setIsApplyingClassId(classId)
      setErrorToast(null)
      setSuccessToast(null)

      await api.createHold(classId, studentId)
      setSuccessToast('Seat held successfully! You have 10 minutes to complete admission.')
      await fetchClassesAndHolds()
      setActiveTab('applications')
    } catch (err: any) {
      setErrorToast(err.message || 'Failed to hold seat')
    } finally {
      setIsApplyingClassId(null)
    }
  }

  // Calculate student states
  const activeHolds = myHolds.filter((h) => h.status === 'HELD')
  const confirmedHolds = myHolds.filter((h) => h.status === 'CONFIRMED')

  return (
    <div className="app-layout">
      {/* Top Navbar */}
      <header className="navbar">
        <div className="navbar-container">
          <div className="flex items-center gap-3">
            <div className="logo-icon-wrapper">
              <GraduationCap className="w-6 h-6 text-indigo-600" />
            </div>
            <div>
              <h1 className="navbar-title">
                Dhaka Model Academy — Admission Portal
              </h1>
              <p className="navbar-subtitle">
                High-Concurrency Seat Allocation & Admission System
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="live-indicator" title="Auto-polling every 3 seconds">
              <span className="live-dot" />
              <span className="text-xs font-medium text-slate-600">
                Live Sync (৩ সে. পর পর)
              </span>
              <button
                type="button"
                onClick={() => fetchClassesAndHolds(true)}
                className="refresh-btn"
                title="Refresh manually"
              >
                <RefreshCw className={`w-3.5 h-3.5 text-slate-500 ${isRefreshing ? 'animate-spin' : ''}`} />
              </button>
            </div>

            <button
              type="button"
              onClick={() => setIsCreateModalOpen(true)}
              className="btn-create-class"
            >
              <Plus className="w-4 h-4" />
              <span>Add Class</span>
            </button>
          </div>
        </div>
      </header>

      {/* Main Container */}
      <main className="main-content">
        {/* Toast Alerts */}
        {errorToast && (
          <div className="toast-alert toast-error">
            <AlertCircle className="w-5 h-5 shrink-0" />
            <div className="flex-1 text-sm font-medium">{errorToast}</div>
            <button onClick={() => setErrorToast(null)} className="toast-close">×</button>
          </div>
        )}

        {successToast && (
          <div className="toast-alert toast-success">
            <CheckCircle className="w-5 h-5 shrink-0 text-emerald-600" />
            <div className="flex-1 text-sm font-medium">{successToast}</div>
            <button onClick={() => setSuccessToast(null)} className="toast-close">×</button>
          </div>
        )}

        {/* Global Student Bar */}
        <StudentBar studentId={studentId} onChange={handleStudentIdChange} />

        {/* Navigation Tabs */}
        <div className="tabs-bar">
          <button
            type="button"
            className={`tab-item ${activeTab === 'classes' ? 'tab-item-active' : ''}`}
            onClick={() => setActiveTab('classes')}
          >
            <span>Available Classes (ভর্তিযোগ্য শ্রেণি)</span>
            <span className="tab-pill">{classes.length}</span>
          </button>

          <button
            type="button"
            className={`tab-item ${activeTab === 'applications' ? 'tab-item-active' : ''}`}
            onClick={() => setActiveTab('applications')}
          >
            <span>My Applications (আমার আবেদন)</span>
            {myHolds.length > 0 && (
              <span className={`tab-pill ${activeHolds.length > 0 ? 'tab-pill-highlight' : ''}`}>
                {myHolds.length}
              </span>
            )}
          </button>
        </div>

        {/* Tab 1: Live Class Cards */}
        {activeTab === 'classes' && (
          <section className="classes-section">
            <div className="section-header">
              <div>
                <h2 className="section-title">Admission Classes & Seat Availability</h2>
                <p className="section-subtitle">
                  Real-time seat counts update dynamically. Holds automatically expire after 10 minutes if unconfirmed.
                </p>
              </div>
              <div className="last-sync-time">
                Updated: {lastUpdated.toLocaleTimeString()}
              </div>
            </div>

            {classes.length === 0 ? (
              <div className="empty-classes-card">
                <GraduationCap className="w-12 h-12 text-slate-300 mx-auto mb-3" />
                <h3 className="text-base font-semibold text-slate-700">No classes configured</h3>
                <p className="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
                  Click the &quot;Add Class&quot; button in the top right to create your first admission class.
                </p>
                <button
                  type="button"
                  onClick={() => setIsCreateModalOpen(true)}
                  className="btn-primary mt-4 inline-flex items-center gap-2"
                >
                  <Plus className="w-4 h-4" /> Create Class
                </button>
              </div>
            ) : (
              <div className="classes-grid">
                {classes.map((cls) => {
                  const hasActiveHold = activeHolds.some((h) => h.classId === cls.id)
                  const hasConfirmed = confirmedHolds.some((h) => h.classId === cls.id)

                  return (
                    <ClassCard
                      key={cls.id}
                      classItem={cls}
                      studentId={studentId}
                      hasActiveHoldInThisClass={hasActiveHold}
                      hasConfirmedInThisClass={hasConfirmed}
                      onApply={handleApply}
                      isApplying={isApplyingClassId === cls.id}
                    />
                  )
                })}
              </div>
            )}
          </section>
        )}

        {/* Tab 2: My Applications */}
        {activeTab === 'applications' && (
          <section className="applications-section">
            <div className="section-header">
              <div>
                <h2 className="section-title">My Admission Status & Applications</h2>
                <p className="section-subtitle">
                  Student Registration: <span className="font-mono font-semibold text-slate-700">{studentId || 'None'}</span>
                </p>
              </div>
              <div className="badge-secure">
                <ShieldCheck className="w-4 h-4 text-emerald-600" />
                <span>Verified Session</span>
              </div>
            </div>

            <MyApplications
              holds={myHolds}
              studentId={studentId}
              onRefresh={() => fetchClassesAndHolds(true)}
            />
          </section>
        )}
      </main>

      {/* Admin Modal */}
      <CreateClassModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onCreated={() => fetchClassesAndHolds(true)}
      />
    </div>
  )
}
export default App
