import { useEffect, useRef } from "react";
import L, { Map as LeafletMap } from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix default marker icon paths for bundlers
import markerIcon2x from "leaflet/dist/images/marker-icon-2x.png";
import markerIcon from "leaflet/dist/images/marker-icon.png";
import markerShadow from "leaflet/dist/images/marker-shadow.png";
import {createMarkers} from "./Markers.ts";
L.Icon.Default.mergeOptions({
    iconRetinaUrl: markerIcon2x,
    iconUrl: markerIcon,
    shadowUrl: markerShadow,
});

export default function Map() {
    const mapDivRef = useRef<HTMLDivElement | null>(null);
    const mapRef = useRef<LeafletMap | null>(null);

    useEffect(() => {
        if (!mapDivRef.current || mapRef.current) return;

        // 1) Init map
        const map = L.map(mapDivRef.current, {
            center: [51.1079, 17.0385], // Wrocław
            zoom: 13,
            preferCanvas: true
        });
        mapRef.current = map;

        // 2) Base layer
        L.tileLayer("https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png", {
            maxZoom: 19,
            updateWhenIdle: true,      // update only after drag/zoom ends
            updateWhenZooming: false,  // don’t refresh tiles during zoom animation
            updateInterval: 250,       // throttle during interaction (ms)
            keepBuffer: 1,             // small offscreen tile buffer
        }).addTo(map);
        
        createMarkers(map);
        
        return () => {
            map.remove();
            mapRef.current = null;
        };
        // Cleanup

    }, []);

    return <div ref={mapDivRef} style={{ height: "100vh", width: "100vw" }} />;
}