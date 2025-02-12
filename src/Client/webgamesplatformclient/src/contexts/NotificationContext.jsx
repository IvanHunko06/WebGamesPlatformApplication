import { createContext, useContext, useState } from "react";

const NotificationContext = createContext();

export const NotificationProvider = ({ children }) => {
    const [notifications, setNotifications] = useState([]);

    const addNotification = (message, type = "info") => {
        const id = Date.now();
        setNotifications((prev) => [...prev, { id, message, type, isVisible: false }]);

        setTimeout(() => {
            setNotifications((prevState) =>
                prevState.map((notification) =>
                    notification.id === id ? { ...notification, isVisible: true } : notification
                )
            );
        }, 50); 

        setTimeout(() => {
            setNotifications((prevState) =>
                prevState.map((notification) =>
                    notification.id === id ? { ...notification, isVisible: false } : notification
                )
            );
        }, 4000); 

        setTimeout(() => {
            setNotifications((prevState) =>
                prevState.filter((notification) => notification.id !== id)
            );
        }, 5000); 
    };

    return (
        <NotificationContext.Provider value={{ addNotification }}>
            {children}
            <NotificationContainer notifications={notifications} />
        </NotificationContext.Provider>
    );
};

const NotificationContainer = ({ notifications }) => {
    return (
        <div className="notification-container">
            {notifications.map((notif, index) => (
                <div
                    key={notif.id}
                    className={`notification ${notif.isVisible ? "visible" : ""} ${
                        notif.type === "error" ? "notification-error" : ""
                    }`}
                    style={{ animationDelay: `${index * 0.1}s` }}
                >
                    {notif.message}
                </div>
            ))}
        </div>
    );
};

export default NotificationProvider;
export const useNotification = () => useContext(NotificationContext);
