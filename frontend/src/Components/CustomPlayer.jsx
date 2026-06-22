import React, { useState, useRef, useEffect } from 'react';
import './CustomPlayer.css';

function CustomPlayer({ src }) {
    const playerRef = useRef(null);
    const [isPlaying, setIsPlaying] = useState(false);

    useEffect(() => {
        const player = playerRef.current;
        const handleStart = () => setIsPlaying(true);
        const handleStop = () => setIsPlaying(false);

        if (player) {
            player.addEventListener('start', handleStart);
            player.addEventListener('stop', handleStop);
        }
        return () => {
            if (player) {
                player.removeEventListener('start', handleStart);
                player.removeEventListener('stop', handleStop);
            }
        };
    }, []);

    const togglePlay = () => {
        if (!playerRef.current) return;
        if (isPlaying) {
            playerRef.current.stop();
        } else {
            playerRef.current.start();
        }
    };

    return (
        <div className="modern-player">
            <midi-player
                ref={playerRef}
                src={src}
                sound-font="https://storage.googleapis.com/magentadata/js/soundfonts/sgm_plus"
                style={{ display: 'none' }}
            ></midi-player>

            <button className="modern-play-btn" onClick={togglePlay}>
                {isPlaying ? (
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                        <rect x="6" y="4" width="4" height="16"/>
                        <rect x="14" y="4" width="4" height="16"/>
                    </svg>
                ) : (
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" style={{ marginLeft: '3px' }}>
                        <polygon points="5,3 19,12 5,21" />
                    </svg>
                )}
            </button>
            
            <div className="modern-equalizer">
                <div className={`bar ${isPlaying ? 'playing' : ''}`} style={{animationDelay: '0s'}}></div>
                <div className={`bar ${isPlaying ? 'playing' : ''}`} style={{animationDelay: '0.2s'}}></div>
                <div className={`bar ${isPlaying ? 'playing' : ''}`} style={{animationDelay: '0.4s'}}></div>
                <div className={`bar ${isPlaying ? 'playing' : ''}`} style={{animationDelay: '0.1s'}}></div>
                <div className={`bar ${isPlaying ? 'playing' : ''}`} style={{animationDelay: '0.3s'}}></div>
            </div>
        </div>
    );
}

export default CustomPlayer;