import * as L from "leaflet";
import {ApiClient} from "./Services/ApiClient";
import { dwarf_svg } from "./assets/dwarf.ts";

export interface Address {
    latitude: number;
    longitude: number;
}

export interface EventTime {
    start: string | number | Date; // ISO string, epoch ms, or Date
    end?: string | number | Date;
}

export interface MapEvent {
    title: MarkerType;
    address: Address;
    description?: string;
    eventTime: EventTime;
}

type MarkerType = "tree" | "deer" | "dwarf" | "santa";


const SYMBOL: Record<MarkerType, string> = {
    tree: "🎄",
    deer: "🦌",
    dwarf: dwarf_svg,
    santa: "🎅",
};

function getTypeIcon(type: MarkerType, cache: Record<string, L.DivIcon>, size = 48,): L.DivIcon {
    const key = `${type}:${size}`;
    if (cache[key]) return cache[key];

    const html = `
    <div>
      <div class="marker-pin ${type}">
      <svg
        width="48" height="48" viewBox="0 0 48 48"
        xmlns="http://www.w3.org/2000/svg"
        style="--pin-stroke: 3;"
        aria-hidden="true" focusable="false">
        <path
                d=" M24 3 Q36 3 39.5 14 Q42 21 37 29 L26 45 Q24 48 22 45 L11 29 Q6 21 8.5 14 Q12 3 24 3 Z"
                fill="currentColor"
                stroke="black"
                stroke-width="var(--pin-stroke)"
                stroke-linejoin="round"
                stroke-linecap="round"
        /
      </svg>
              <span class="symbol">${SYMBOL[type]}</span>
      </div>
    </div>
  `;

    const icon = L.divIcon({
        html,
        className: "",                  // keep DOM/CSS minimal
        iconSize: [size, size],
        iconAnchor: [size / 2, size],   // bottom-center
        // NOTE: shadowUrl is ignored by L.divIcon (only used by L.icon)
    });

    cache[key] = icon;
    return icon;
}


export async function createMarkers(map: L.Map,) {
    const ICON_CACHE: Record<string, L.DivIcon> = {};
    const events = await new ApiClient({baseUrl: "/api"}).get<MapEvent[]>("http://localhost:5000/Api/events");

    // Remove existing L.Marker layers
    map.eachLayer((layer: L.Layer) => {
        if (layer instanceof L.Marker) {
            map.removeLayer(layer);
        }
    });


    for (const event of events) {
        const {title, address} = event;

        L.marker([address.latitude, address.longitude], {icon: getTypeIcon(title, ICON_CACHE)}).addTo(map);
    }
}