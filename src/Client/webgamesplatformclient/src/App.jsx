import MainApp from './MainApp';
import { useAuth } from './contexts/AuthContext';
function App() {
  const {isLogged} = useAuth()
  return (
    <>
    {isLogged? (
      <MainApp/>
    ):(
      <p>authentication in progress</p>
    )}

    </>

  );
}

export default App;
