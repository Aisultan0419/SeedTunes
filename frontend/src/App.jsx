import React, { useState, useEffect, useCallback, useRef } from 'react';
import ToolBar from './Components/Toolbar.jsx';
import SongsTable from './Components/SongsTable.jsx';
import GalleryView from './Components/GalleryView.jsx';
import './App.css';

function App() {
  const [language, setLanguage] = useState('en');
  const [seed, setSeed] = useState("13584923745198220456");
  const [likes, setLikes] = useState(5);
  const [debouncedLikes, setDebouncedLikes] = useState(5); 
  const [viewMode, setViewMode] = useState('table');
  const [pageNumber, setPageNumber] = useState(1);
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);

  const generateRandomSeed = () => {
    const randomSeed = Math.floor(Math.random() * Number.MAX_SAFE_INTEGER).toString();
    setSeed(randomSeed);
  };

  const fetchData = useCallback(async (currentLanguage, currentSeed, currentLikes, currentPage, isGalleryAppend) => {
    setLoading(true);
    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL || "";
      const response = await fetch(`${baseUrl}/api/songs/page?userSeed=${currentSeed}&pageNumber=${currentPage}&averageLikes=${currentLikes}&languageCode=${currentLanguage}`);
      const result = await response.json();
      
      const newSongs = Array.isArray(result) ? result : (result.songs || result.items || []);
      
      if (isGalleryAppend) {
        setData(prev => [...prev, ...newSongs]);
      } else {
        setData(newSongs);
      }
    } catch (error) {
      console.error("Error fetching data:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  const prevFilters = useRef({ language, seed, likes: debouncedLikes });

  // Эффект для дебаунса ползунка лайков
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedLikes(likes);
    }, 350); 

    return () => clearTimeout(handler); 
  }, [likes]);


  useEffect(() => {
    let currentPage = pageNumber;
    let isGalleryAppend = viewMode === 'gallery' && pageNumber > 1;

    if (
      prevFilters.current.language !== language ||
      prevFilters.current.seed !== seed ||
      prevFilters.current.likes !== debouncedLikes
    ) {
      currentPage = 1;
      isGalleryAppend = false;
      setPageNumber(1);
      prevFilters.current = { language, seed, likes: debouncedLikes };
    }

    fetchData(language, seed, debouncedLikes, currentPage, isGalleryAppend);
  }, [language, seed, debouncedLikes, pageNumber, viewMode, fetchData]);

  const handleNextPage = () => setPageNumber(prev => prev + 1);
  const handlePrevPage = () => setPageNumber(prev => Math.max(1, prev - 1));

  return (
    <div className="app-container">
      <ToolBar 
        language={language} setLanguage={setLanguage}
        seed={seed} setSeed={setSeed} onRandomSeed={generateRandomSeed}
        likes={likes} setLikes={setLikes}
        viewMode={viewMode} setViewMode={setViewMode}
      />
      {viewMode === 'table' ? (
        <SongsTable 
          data={data} 
          pageNumber={pageNumber} 
          onNextPage={handleNextPage} 
          onPrevPage={handlePrevPage} 
          loading={loading}
          seed={seed}
        />
      ) : (
        <GalleryView 
          data={data} 
          onLoadMore={handleNextPage} 
          loading={loading}
          seed={seed}
        />
      )}
    </div>
  );
}

export default App;