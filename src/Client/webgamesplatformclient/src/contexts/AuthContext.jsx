import Keycloak from 'keycloak-js';
import {createContext, useContext, useRef, useCallback, useMemo, useEffect, useState } from 'react';

const AuthContext = createContext();
 // eslint-disable-next-line react/prop-types
 const AuthProvider = ({children}) => {
    const [isLogged, setIsLogged] = useState(false);
    let kc = useRef(null);



    const login = useCallback((initOptions)=>{
      kc.current = new Keycloak(initOptions);
      kc.current.init({
        onLoad: 'login-required',
        pkceMethod: "S256",
        checkLoginIframe: false
      }).then((auth)=>{
        if(!auth){
          window.location.reload();
        }else{
          setIsLogged(true)
          console.log("Authentificated");
          console.log("token:", kc.current.token);
          kc.current.onTokenExpired = () =>{
            console.log("token expired")
            kc.current.updateToken(30).success(() => {
              console.log('successfully get a new token', kc.current.token);
            })
            .error(() => {
              console.log("failed to get new token ");
            });
          }
        }
      });

    },[]);
    const logout = useCallback(()=>{
      setIsLogged(false);
      kc.current.logout({
        redirectUri: 'http://localhost:5173/'
      });
    },[]);
    const getToken = useCallback(()=>{
      if(isLogged){
        return kc.current.token;
      }
      else{
        return "";
      }
    }, [isLogged]);
    const authContext = useMemo(()=>({
      isLogged,
      logout,
      getToken
    }), [isLogged, logout, getToken]);

    useEffect(()=>{
      if(!kc.current){
        login({
          url: 'https://localhost:7005/auth/',
          realm: 'WebGamesPlatform',
          clientId: 'public-client',
        });
      }
    }, [login]);

    return (
      <AuthContext.Provider value={authContext}>
        {children}
      </AuthContext.Provider>
    )

 };
export default AuthProvider;
// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => useContext(AuthContext);