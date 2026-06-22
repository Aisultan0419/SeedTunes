import './Toolbar.css'

function ToolBar({ language, setLanguage, seed, setSeed, onRandomSeed, likes, setLikes, viewMode, setViewMode }) {
    return (
        <div className="toolbar">
            <div className="select-container">
                <label className="select-label">Language</label>
                <select className="language-selector" value={language} onChange={(e) => setLanguage(e.target.value)}>
                    <option value="en">English (US)</option>
                    <option value="de">Germany</option>
                </select>  
            </div>
            <div className="seed-container">
                <label className="seed-label">Seed</label>
                <div style={{display: 'flex', height: '100%'}}>
                    <input className="seed-number" type="number" value={seed} onChange={(e) => setSeed(e.target.value)} />
                    <button className="random-seed-btn" onClick={onRandomSeed} title="Random Seed">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <polyline points="16 3 21 3 21 8"></polyline>
                            <line x1="4" y1="20" x2="21" y2="3"></line>
                            <polyline points="21 16 21 21 16 21"></polyline>
                            <line x1="15" y1="15" x2="21" y2="21"></line>
                            <line x1="4" y1="4" x2="9" y2="9"></line>
                        </svg>
                    </button>
                </div>
            </div> 
            <div className="like-container"> 
                <label className="like-label">Likes: {likes}</label>
                <div className="slider-ticks">
                    <span></span><span></span><span></span><span></span><span></span>
                    <span></span><span></span><span></span><span></span><span></span>
                </div>
                <input className="like-slider" type="range" min="0" max="10" value={likes} step={0.1} onChange={(e) => setLikes(parseFloat(e.target.value))} />
            </div>
            <div className="switch-view">
                <input type="radio" name="viewMode" className="table-view" id="table-view" checked={viewMode === 'table'} onChange={() => setViewMode('table')} />
                <label className="table-label" htmlFor="table-view">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="3" y="3" width="18" height="18" rx="2"/>
                        <line x1="9" y1="3" x2="9" y2="21"/>
                        <line x1="15" y1="3" x2="15" y2="21"/>
                        <line x1="3" y1="9" x2="21" y2="9"/>
                        <line x1="3" y1="15" x2="21" y2="15"/>
                    </svg>
                </label>

                <input type="radio" name="viewMode" className="gallery-view" id="gallery-view" checked={viewMode === 'gallery'} onChange={() => setViewMode('gallery')} />
                <label className="gallery-label" htmlFor="gallery-view">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="3" y="3" width="7" height="7"/>
                        <rect x="14" y="3" width="7" height="7"/>
                        <rect x="14" y="14" width="7" height="7"/>
                        <rect x="3" y="14" width="7" height="7"/>
                    </svg>
                </label>
            </div>
        </div>
    );
}
export default ToolBar;