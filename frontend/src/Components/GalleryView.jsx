import React, { useRef, useCallback } from 'react';
import './GalleryView.css';
import CustomPlayer from './CustomPlayer.jsx';

function GalleryView({ data, onLoadMore, loading }) {
    const observer = useRef();
    const baseUrl = import.meta.env.VITE_API_BASE_URL || "";

    const lastElementRef = useCallback(node => {
        if (loading) return;
        if (observer.current) observer.current.disconnect();
        observer.current = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting) {
                onLoadMore();
            }
        });
        if (node) observer.current.observe(node);
    }, [loading, onLoadMore]);

    return (
        <div className="gallery-view-container">
            <div className="gallery-grid">
                {data.map((song, idx) => {
                    const isLastElement = data.length === idx + 1;
                    return (
                        <div 
                            key={idx} 
                            className="gallery-card"
                            ref={isLastElement ? lastElementRef : null}
                        >
                            <div className="gallery-cover">
                                {song.coverUrl ? (
                                    <img src={song.coverUrl} alt="Cover" />
                                ) : (
                                    <div className="gallery-cover-placeholder">Cover</div>
                                )}
                            </div>
                            <div className="gallery-info">
                                <h4 className="gallery-title">{song.title}</h4>
                                <p className="gallery-artist">{song.artist}</p>
                                <p className="gallery-album">{song.album}</p>
                                <div className="gallery-likes" style={{ marginTop: '10px' }}>
                                    <span className="likes-badge">
                                        <svg viewBox="0 0 24 24">
                                            <path d="M2 20h2c.55 0 1-.45 1-1v-9c0-.55-.45-1-1-1H2v11zm19.83-7.12c.11-.25.17-.52.17-.8V11c0-1.1-.9-2-2-2h-5.5l.92-4.65c.05-.22.02-.46-.08-.66-.23-.45-.52-.86-.88-1.22L14 2 7.59 8.41C7.21 8.79 7 9.3 7 9.83v7.84C7 18.95 8.05 20 9.34 20h8.11c.7 0 1.36-.37 1.72-.97l2.66-6.15z"/>
                                        </svg>
                                        {song.likeCount}
                                    </span>
                                </div>
                                <div style={{marginTop: '10px'}}>
                                    <CustomPlayer src={`${baseUrl}${song.audioUrl}`} />
                                </div>
                            </div>
                        </div>
                    );
                })}
            </div>
            {loading && <div className="gallery-loading">Loading more songs...</div>}
        </div>
    );
}

export default GalleryView;