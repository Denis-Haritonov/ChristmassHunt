// src/services/api.ts
export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export interface ApiClientOptions {
    /** Base URL like "/api" or "https://example.com/api" */
    baseUrl?: string;
    /** Default headers for every request (e.g., { Authorization: `Bearer ...` }) */
    defaultHeaders?: Record<string, string>;
}

export interface RequestOptions {
    signal?: AbortSignal;
    headers?: Record<string, string>;
    /** Pass a plain object for JSON; omit for no body */
    body?: unknown;
}

export class ApiClient {
    private baseUrl: string;
    private defaultHeaders: Record<string, string>;

    constructor(opts: ApiClientOptions = {}) {
        this.baseUrl = (opts.baseUrl ?? "").replace(/\/+$/, "");
        this.defaultHeaders = opts.defaultHeaders ?? {};
    }

    get<T>(path: string, opts: RequestOptions = {}): Promise<T> {
        return this.request<T>("GET", path, opts);
    }

    post<T>(path: string, opts: RequestOptions = {}): Promise<T> {
        return this.request<T>("POST", path, opts);
    }

    put<T>(path: string, opts: RequestOptions = {}): Promise<T> {
        return this.request<T>("PUT", path, opts);
    }

    patch<T>(path: string, opts: RequestOptions = {}): Promise<T> {
        return this.request<T>("PATCH", path, opts);
    }

    delete<T>(path: string, opts: RequestOptions = {}): Promise<T> {
        return this.request<T>("DELETE", path, opts);
    }

    private async request<T>(method: HttpMethod, path: string, opts: RequestOptions): Promise<T> {
        const url = this.buildUrl(path);

        const headers: Record<string, string> = {
            Accept: "application/json",
            ...this.defaultHeaders,
            ...opts.headers,
        };

        const init: RequestInit = { method, headers, signal: opts.signal };

        if (opts.body !== undefined) {
            headers["Content-Type"] = headers["Content-Type"] ?? "application/json";
            init.body = headers["Content-Type"] === "application/json"
                ? JSON.stringify(opts.body)
                : (opts.body as any);
        }

        const res = await fetch(url, init);

        if (!res.ok) {
            // Try to include server message if it’s JSON or text
            let details = "";
            try {
                const ct = res.headers.get("content-type") ?? "";
                if (ct.includes("application/json")) {
                    const j = await res.json();
                    details = ` — ${JSON.stringify(j)}`;
                } else {
                    const t = await res.text();
                    details = t ? ` — ${t}` : "";
                }
            } catch { /* ignore */ }

            throw new Error(`${res.status} ${res.statusText}${details}`);
        }

        // 204 No Content or empty body
        if (res.status === 204) return undefined as unknown as T;

        // Assume JSON responses
        const ct = res.headers.get("content-type") ?? "";
        if (!ct.includes("application/json")) {
            // If server sent plain text, return it as any
            const text = await res.text();
            return text as unknown as T;
        }

        return res.json() as Promise<T>;
    }

    private buildUrl(path: string): string {
        // absolute path or full URL
        if (/^https?:\/\//i.test(path)) return path;
        if (!this.baseUrl) return path;
        // ensure exactly one slash
        return `${this.baseUrl}${path.startsWith("/") ? "" : "/"}${path}`;
    }
}