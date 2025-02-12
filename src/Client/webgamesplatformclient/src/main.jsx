import { createRoot } from 'react-dom/client'
import App from './App.jsx'
import AuthProvider from './contexts/AuthContext.jsx'
import JwtProvider from './contexts/JwtTokenContext.jsx'
import NotificationProvider from './contexts/NotificationContext.jsx'

createRoot(document.getElementById('root')).render(
  <AuthProvider>
    <JwtProvider>
      <NotificationProvider>
      <App />
      </NotificationProvider>
    </JwtProvider>
  </AuthProvider>
)
