import { Link } from 'react-router-dom';
import { useEffect } from 'react';
import './HomePage.css'
import { useJwt } from '../../contexts/JwtTokenContext';
export default function HomePage() {

    const { getUsername } = useJwt();
    useEffect(() => {
        document.title = "Home";
    }, []);

    return (
        <div className="page-home">
            <div className="ag-format-container">
                <div className="ag-courses_grid">
                    <div className="ag-courses_item large-item">
                        <Link to="/games" className="ag-courses-item_link">
                            <div className="ag-courses-item_bg1"></div>
                            <div className="ag-courses-item_title">Our games</div>
                        </Link>
                    </div>
                    <div className="ag-courses_item">
                        <Link to={`/profile/${getUsername()}`} className="ag-courses-item_link">
                            <div className="ag-courses-item_bg"></div>
                            <div className="ag-courses-item_title">Your profile</div>
                        </Link>
                    </div>
                    <div className="ag-courses_item">
                        <Link to="/leaderboard" className="ag-courses-item_link">
                            <div className="ag-courses-item_bg"></div>
                            <div className="ag-courses-item_title">Leaders Rating</div>
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}