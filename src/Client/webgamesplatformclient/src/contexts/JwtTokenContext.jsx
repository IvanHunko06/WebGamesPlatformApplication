import React, { createContext, useContext, useMemo, useState, useEffect, useCallback } from 'react';
import {jwtDecode} from 'jwt-decode';
import { useAuth } from '../contexts/AuthContext';

const JwtContext = createContext();

export const JwtProvider = ({ children }) => {
    const { getToken, isLogged } = useAuth();
    const [decodedToken, setDecodedToken] = useState(null);

    const decodeToken = useCallback(async () => {
        try {
            const token = await getToken(); 
            if (token) {
                const decoded = jwtDecode(token);
                setDecodedToken(decoded);
            } else {
                setDecodedToken(null);
            }
        } catch (error) {
            console.error("Error decoding token:", error);
            setDecodedToken(null);
        }
    }, [getToken]);

    useEffect(() => {
        if (isLogged) {
            decodeToken(); 
        } else {
            setDecodedToken(null);
        }
    }, [isLogged, decodeToken]);

    const getUsername = useCallback(() => {
        return decodedToken?.preferred_username || "Guest";
    }, [decodedToken]);

    const getEmail = useCallback(() => {
        return decodedToken?.email || "No Email";
    }, [decodedToken]);

    const value = useMemo(() => ({
        getUsername,
        getEmail,
        decodedToken, 
    }), [getUsername, getEmail, decodedToken]);

    return (
        <JwtContext.Provider value={value}>
            {children}
        </JwtContext.Provider>
    );
};

export default JwtProvider;

export const useJwt = () => useContext(JwtContext);
