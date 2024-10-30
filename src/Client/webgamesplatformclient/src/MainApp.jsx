import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { SignalRProvider } from './contexts/SignalRContext'
import { useAuth } from './contexts/AuthContext';
function MainApp(){
    const [count, setCount] = useState(0)
    const hubUrlOne = 'https://localhost:7005/api/hubs/main-hub';
    const {logout, getToken} = useAuth()
    let token = getToken()
    return (
        <SignalRProvider hubUrl={hubUrlOne} jwtToken={token}>
            <div>
            <a href="https://vitejs.dev" target="_blank">
                <img src={viteLogo} className="logo" alt="Vite logo" />
            </a>
            <a href="https://react.dev" target="_blank">
                <img src={reactLogo} className="logo react" alt="React logo" />
            </a>
            </div>
            <h1>Vite + React</h1>
            <div className="card">
            <button onClick={() => setCount((count) => count + 1)}>
                count is {count}
            </button>
            <p>
                Edit <code>src/App.jsx</code> and save to test HMR
            </p>
            </div>
            <p className="read-the-docs">
            {token}
            </p>
            <button onClick={() => logout()}>
                Logout
            </button>
        </SignalRProvider>
    );
}

export default MainApp;