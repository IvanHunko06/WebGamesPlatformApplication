import MainApp from './MainApp';
import { useAuth } from './contexts/AuthContext';
function App() {
  const {isLogged} = useAuth()
  return (
    <>
    {isLogged? (
      <MainApp/>
    ):(
      <div className="loading">Authentication in progress</div>
    )}
    </>

  );
}

export default App;
