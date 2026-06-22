import './SongsTable.css';
import { useState } from 'react';
import CustomPlayer from './CustomPlayer.jsx';

function SongsTable({ data, pageNumber, onNextPage, onPrevPage, loading }) {
    const [expandedRow, setExpandedRow] = useState(null);
    const baseUrl = import.meta.env.VITE_API_BASE_URL || "";

    const toggleRow = (index) => {
        setExpandedRow(expandedRow === index ? null : index);
    };

    return (
        <div className="songs-table">
            <div className="table-header">
                <span>#</span>
                <span>Song</span>
                <span>Artist</span>
                <span>Album</span>
                <span>Genre</span>
            </div>
            <hr />
            <div className="table-body">
                {data.map((song, idx) => (
                    <div key={idx} className="table-row-container">
                        <div className="table-row" onClick={() => toggleRow(idx)}>
                            <span className="expand-icon">{expandedRow === idx ? '▼' : '▶'}</span>
                            <span>{song.index}</span>
                            <span>{song.title}</span>
                            <span>{song.artist}</span>
                            <span>{song.album}</span>
                            <span>{song.genre}</span>
                        </div>
                        
                        {expandedRow === idx && (
                            <div className="expanded-details-wrapper">
                                <div className="expanded-details">
                                    <div className="cover-container">
                                        {song.coverUrl ? (
                                            <img src={song.coverUrl} alt="Cover" className="album-cover" />
                                        ) : (
                                            <div className="album-cover-placeholder">No Cover</div>
                                        )}
                                    </div>
                                    <div className="details-info">
                                        <div className="details-header">
                                            <h3>{song.title}</h3>
                                            <CustomPlayer src={`${baseUrl}${song.audioUrl}`} />
                                        </div>
                                        <p className="artist-album-info">from <strong>{song.album}</strong> by <strong>{song.artist}</strong></p>
                                        
                                        <div className="likes-info">
                                            <span className="likes-badge">
                                                <svg viewBox="0 0 24 24">
                                                    <path d="M2 20h2c.55 0 1-.45 1-1v-9c0-.55-.45-1-1-1H2v11zm19.83-7.12c.11-.25.17-.52.17-.8V11c0-1.1-.9-2-2-2h-5.5l.92-4.65c.05-.22.02-.46-.08-.66-.23-.45-.52-.86-.88-1.22L14 2 7.59 8.41C7.21 8.79 7 9.3 7 9.83v7.84C7 18.95 8.05 20 9.34 20h8.11c.7 0 1.36-.37 1.72-.97l2.66-6.15z"/>
                                                </svg>
                                                {song.likeCount}
                                            </span>
                                        </div>
                                        
                                        <div className="review-text">
                                            <p>{song.description}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                ))}
                {data.length === 0 && !loading && <div className="no-data">No songs found.</div>}
            </div>
            <div className="pagination">
                <button onClick={onPrevPage} disabled={pageNumber === 1 || loading}>&laquo; Prev</button>
                <span className="page-number">{pageNumber}</span>
                <button onClick={onNextPage} disabled={loading}>Next &raquo;</button>
            </div>
        </div>
    );
}

export default SongsTable;