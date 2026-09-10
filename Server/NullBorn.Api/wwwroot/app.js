(() => {
  const ZONE_ID = "arttest";
  const PLAYER_ID = "local-player";

  const statusEl = document.getElementById("status");
  const statusPill = document.getElementById("statusPill");
  const canvas = document.getElementById("overlay");
  const ctx = canvas.getContext("2d");
  const listEl = document.getElementById("markerList");
  const wrap = document.getElementById("mapWrap");
  const mapBg = document.getElementById("mapBg");
  const lanHint = document.getElementById("lanHint");
  const zoneNameEl = document.getElementById("zoneName");
  const markerCountEl = document.getElementById("markerCount");
  const poseReadoutEl = document.getElementById("poseReadout");
  const markersSubtitle = document.getElementById("markersSubtitle");
  const btnCopyUrl = document.getElementById("btnCopyUrl");
  const apiHint = document.getElementById("apiHint");
  const shareLabel = document.getElementById("shareLabel");

  function apiBase() {
    return String(window.NULLBORN_API_BASE || "").trim().replace(/\/$/, "");
  }

  function apiUrl(path) {
    const p = path.startsWith("/") ? path : `/${path}`;
    const base = apiBase();
    return base ? `${base}${p}` : p;
  }

  let zone = null;
  let markers = [];
  let player = { x: 0, z: -3, yaw: 0 };
  let display = { x: 0, z: -3, yaw: 0 };
  let longPressTimer = null;
  let longPressFired = false;
  let pointerDown = null;
  let busy = false;
  let lastTouchAt = 0;
  let rafId = 0;
  let lastDraw = 0;

  function setStatus(text, state) {
    statusEl.textContent = text;
    if (statusPill) statusPill.dataset.state = state || "pending";
  }

  function normalizePose(pose) {
    if (!pose) return null;
    return {
      playerId: pose.playerId ?? pose.PlayerId,
      x: Number(pose.x ?? pose.X),
      z: Number(pose.z ?? pose.Z),
      yaw: Number(pose.yaw ?? pose.Yaw)
    };
  }

  function updatePoseReadout() {
    if (poseReadoutEl) {
      poseReadoutEl.textContent = `${display.x.toFixed(1)}, ${display.z.toFixed(1)} · ${display.yaw.toFixed(0)}°`;
    }
  }

  function updateMarkerMeta() {
    if (markerCountEl) markerCountEl.textContent = String(markers.length);
    if (markersSubtitle) {
      markersSubtitle.textContent = markers.length
        ? `${markers.length} waypoint${markers.length === 1 ? "" : "s"} en la zona`
        : "Sin waypoints todavía";
    }
  }

  function worldToCanvas(x, z) {
    const w = canvas.width;
    const h = canvas.height;
    const u = (x - zone.minX) / (zone.maxX - zone.minX);
    // +Z (Unity north) → top of map
    const v = 1 - (z - zone.minZ) / (zone.maxZ - zone.minZ);
    return { px: u * w, py: v * h };
  }

  function canvasToWorld(px, py) {
    const w = canvas.width;
    const h = canvas.height;
    const u = px / w;
    const v = 1 - py / h;
    return {
      x: zone.minX + u * (zone.maxX - zone.minX),
      z: zone.minZ + v * (zone.maxZ - zone.minZ)
    };
  }

  function resizeCanvas() {
    const rect = wrap.getBoundingClientRect();
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = Math.floor(rect.width * dpr);
    canvas.height = Math.floor(rect.height * dpr);
    canvas.style.width = `${rect.width}px`;
    canvas.style.height = `${rect.height}px`;
    scheduleDraw(true);
  }

  function lerp(a, b, t) {
    return a + (b - a) * t;
  }

  function lerpAngle(a, b, t) {
    let d = ((b - a + 540) % 360) - 180;
    return a + d * t;
  }

  function tickDisplay(dt) {
    const t = 1 - Math.exp(-12 * dt);
    display.x = lerp(display.x, player.x, t);
    display.z = lerp(display.z, player.z, t);
    display.yaw = lerpAngle(display.yaw, player.yaw, t);
  }

  function drawPlayerArrow(px, py, yawDeg, scale) {
    // Unity yaw 0 = +Z = up on map. Positive yaw turns toward +X (right) → clockwise on screen.
    ctx.save();
    ctx.translate(px, py);
    ctx.rotate((yawDeg * Math.PI) / 180);

    const s = scale;
    // Soft glow
    ctx.beginPath();
    ctx.fillStyle = "rgba(125, 206, 130, 0.25)";
    ctx.arc(0, 0, 18 * s, 0, Math.PI * 2);
    ctx.fill();

    // Arrow tip toward -Y (up / north when yaw=0)
    ctx.beginPath();
    ctx.moveTo(0, -14 * s);
    ctx.lineTo(10 * s, 12 * s);
    ctx.lineTo(0, 6 * s);
    ctx.lineTo(-10 * s, 12 * s);
    ctx.closePath();
    ctx.fillStyle = "#7dce82";
    ctx.strokeStyle = "#0a120c";
    ctx.lineWidth = 2 * s;
    ctx.fill();
    ctx.stroke();
    ctx.restore();
  }

  function drawMarkers(scale) {
    for (const m of markers) {
      const p = worldToCanvas(m.x, m.z);
      const r = (m.isActive ? 13 : 9) * scale;

      if (m.isActive) {
        ctx.beginPath();
        ctx.strokeStyle = "rgba(224, 145, 74, 0.55)";
        ctx.lineWidth = 2 * scale;
        ctx.arc(p.px, p.py, r + 8 * scale, 0, Math.PI * 2);
        ctx.stroke();
      }

      ctx.beginPath();
      ctx.fillStyle = m.color || "#e85d04";
      // Diamond pin
      ctx.moveTo(p.px, p.py - r);
      ctx.lineTo(p.px + r, p.py);
      ctx.lineTo(p.px, p.py + r);
      ctx.lineTo(p.px - r, p.py);
      ctx.closePath();
      ctx.fill();
      if (m.isActive) {
        ctx.strokeStyle = "#fff";
        ctx.lineWidth = 2.5 * scale;
        ctx.stroke();
      }

      ctx.fillStyle = "rgba(231, 239, 228, 0.95)";
      ctx.font = `600 ${Math.max(11, 12 * scale)}px Manrope, system-ui, sans-serif`;
      ctx.fillText(m.label || "Pin", p.px + r + 6 * scale, p.py - 4 * scale);
    }
  }

  function draw() {
    if (!zone) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    const scale = canvas.width / 400;
    drawMarkers(scale);
    const pp = worldToCanvas(display.x, display.z);
    drawPlayerArrow(pp.px, pp.py, display.yaw, scale);
    updatePoseReadout();
  }

  function scheduleDraw(force) {
    const now = performance.now();
    if (!force && now - lastDraw < 32) return;
    lastDraw = now;
    draw();
  }

  function loop(ts) {
    if (!loop._last) loop._last = ts;
    const dt = Math.min(0.05, (ts - loop._last) / 1000);
    loop._last = ts;
    tickDisplay(dt);
    scheduleDraw(false);
    rafId = requestAnimationFrame(loop);
  }

  function renderList() {
    listEl.innerHTML = "";
    updateMarkerMeta();
    if (!markers.length) {
      listEl.innerHTML = `<li class="empty">Toca el mapa para crear tu primer pin</li>`;
      return;
    }

    for (const m of markers) {
      const li = document.createElement("li");
      if (m.isActive) li.classList.add("active");
      li.innerHTML = `
        <div>
          <span class="dot" style="background:${m.color}"></span>
          <strong>${escapeHtml(m.label)}</strong>
          <div class="meta">${m.x.toFixed(1)}, ${m.z.toFixed(1)}${m.isActive ? " · destino activo" : ""}</div>
        </div>
        <div class="actions">
          <button type="button" class="primary" data-act="activate" data-id="${m.id}">Ir</button>
          <button type="button" class="danger" data-act="delete" data-id="${m.id}">Borrar</button>
        </div>`;
      listEl.appendChild(li);
    }
  }

  function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, (c) => ({
      "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[c]));
  }

  function nextPinLabel() {
    let max = 0;
    for (const m of markers) {
      const match = /^Pin\s+(\d+)$/i.exec((m.label || "").trim());
      if (match) max = Math.max(max, Number(match[1]));
    }
    return `Pin ${max + 1}`;
  }

  async function api(path, options) {
    const headers = { "Content-Type": "application/json", ...(options && options.headers) };
    if (apiBase().includes("ngrok"))
      headers["ngrok-skip-browser-warning"] = "1";
    const res = await fetch(apiUrl(path), {
      headers,
      ...options
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || res.statusText);
    }
    if (res.status === 204) return null;
    return res.json();
  }

  function applyMapBackground(z) {
    if (!mapBg || !z) return;
    const url = z.mapImageUrl || "/maps/arttest.svg";
    const abs = url.startsWith("http") ? url : apiUrl(url);
    mapBg.style.backgroundImage = `url("${abs}?v=3")`;
    mapBg.style.backgroundSize = "cover";
    mapBg.style.backgroundPosition = "center";
    mapBg.style.backgroundRepeat = "no-repeat";
  }

  async function loadZone() {
    zone = await api(`/api/zones/${ZONE_ID}`);
    // Normalize casing from API
    zone = {
      ...zone,
      minX: zone.minX ?? zone.MinX,
      maxX: zone.maxX ?? zone.MaxX,
      minZ: zone.minZ ?? zone.MinZ,
      maxZ: zone.maxZ ?? zone.MaxZ,
      mapImageUrl: zone.mapImageUrl ?? zone.MapImageUrl,
      name: zone.name ?? zone.Name
    };
    if (zoneNameEl) zoneNameEl.textContent = zone.name || ZONE_ID;
    const mapTitle = document.getElementById("mapTitle");
    if (mapTitle) mapTitle.textContent = zone.name || "Mapa táctico";
    if (lanHint) lanHint.textContent = `${location.protocol}//${location.host}`;
    if (apiHint) apiHint.textContent = apiBase() || "misma origen (API local)";
    if (shareLabel) {
      shareLabel.textContent = apiBase()
        ? "Página en Render — los datos van a tu API local (túnel)"
        : "Abre esta página en el móvil (misma Wi‑Fi)";
    }
    applyMapBackground(zone);
  }

  async function loadMarkers() {
    markers = await api(`/api/markers?zoneId=${encodeURIComponent(ZONE_ID)}`);
    renderList();
    scheduleDraw(true);
  }

  function applyPlayerPose(p) {
    const n = normalizePose(p);
    if (!n || Number.isNaN(n.x)) return;
    player = { x: n.x, z: n.z, yaw: n.yaw };
  }

  async function loadPlayer() {
    try {
      const p = await api(`/api/player/${PLAYER_ID}`);
      applyPlayerPose(p);
      display = { ...player };
      scheduleDraw(true);
    } catch (_) { /* first boot ok */ }
  }

  async function createMarker(x, z) {
    if (busy) return;
    for (const m of markers) {
      if (Math.hypot(m.x - x, m.z - z) < 1.25) return;
    }

    busy = true;
    try {
      const label = nextPinLabel();
      const created = await api("/api/markers", {
        method: "POST",
        body: JSON.stringify({ zoneId: ZONE_ID, label, x, z, color: "#e85d04", type: "nav" })
      });
      if (created && !markers.some((m) => m.id === created.id)) {
        markers = [...markers, created];
        renderList();
        scheduleDraw(true);
      } else {
        await loadMarkers();
      }
    } finally {
      setTimeout(() => { busy = false; }, 350);
    }
  }

  async function activateMarker(id) {
    if (busy) return;
    busy = true;
    try {
      await api(`/api/markers/${id}`, {
        method: "PATCH",
        body: JSON.stringify({ isActive: true })
      });
      await loadMarkers();
    } finally {
      busy = false;
    }
  }

  async function deleteMarker(id) {
    if (busy) return;
    busy = true;
    try {
      await api(`/api/markers/${id}`, { method: "DELETE" });
      markers = markers.filter((m) => m.id !== id);
      renderList();
      scheduleDraw(true);
    } finally {
      busy = false;
    }
  }

  function eventToCanvas(e) {
    const rect = canvas.getBoundingClientRect();
    const touch = e.changedTouches ? e.changedTouches[0] : e;
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    return {
      px: (touch.clientX - rect.left) * scaleX,
      py: (touch.clientY - rect.top) * scaleY
    };
  }

  function findMarkerNear(px, py) {
    const threshold = 28 * (window.devicePixelRatio || 1);
    let best = null;
    let bestDist = threshold;
    for (const m of markers) {
      const p = worldToCanvas(m.x, m.z);
      const d = Math.hypot(p.px - px, p.py - py);
      if (d < bestDist) {
        best = m;
        bestDist = d;
      }
    }
    return best;
  }

  function isGhostMouse(e) {
    return e.type.startsWith("mouse") && (Date.now() - lastTouchAt) < 700;
  }

  async function handlePointerUp(e) {
    if (isGhostMouse(e)) return;
    clearTimeout(longPressTimer);

    if (longPressFired) {
      longPressFired = false;
      pointerDown = null;
      return;
    }

    if (!pointerDown) return;
    const { px, py } = eventToCanvas(e);
    const moved = Math.hypot(px - pointerDown.px, py - pointerDown.py);
    pointerDown = null;
    if (moved > 24 * (window.devicePixelRatio || 1)) return;

    const hit = findMarkerNear(px, py);
    if (hit) {
      await activateMarker(hit.id);
      return;
    }
    const world = canvasToWorld(px, py);
    await createMarker(world.x, world.z);
  }

  function bindMapInput() {
    const onStart = (e) => {
      if (isGhostMouse(e)) return;
      if (e.type.startsWith("touch")) lastTouchAt = Date.now();

      longPressFired = false;
      const pos = eventToCanvas(e);
      pointerDown = pos;
      const hit = findMarkerNear(pos.px, pos.py);
      longPressTimer = setTimeout(async () => {
        longPressFired = true;
        pointerDown = null;
        if (hit) await deleteMarker(hit.id);
      }, 550);
    };

    const onCancel = () => {
      clearTimeout(longPressTimer);
      pointerDown = null;
    };

    canvas.addEventListener("touchstart", onStart, { passive: true });
    canvas.addEventListener("touchend", (e) => {
      lastTouchAt = Date.now();
      e.preventDefault();
      handlePointerUp(e);
    }, { passive: false });
    canvas.addEventListener("touchcancel", onCancel);
    canvas.addEventListener("mousedown", onStart);
    canvas.addEventListener("mouseup", handlePointerUp);
  }

  listEl.addEventListener("click", async (e) => {
    const btn = e.target.closest("button[data-act]");
    if (!btn) return;
    const id = btn.getAttribute("data-id");
    if (btn.dataset.act === "activate") await activateMarker(id);
    if (btn.dataset.act === "delete") await deleteMarker(id);
  });

  const btnRefresh = document.getElementById("btnRefresh");
  if (btnRefresh) {
    btnRefresh.addEventListener("click", async () => {
      const prev = btnRefresh.textContent;
      btnRefresh.textContent = "…";
      try {
        await loadMarkers();
        await loadPlayer();
        btnRefresh.textContent = "Listo";
      } catch (_) {
        btnRefresh.textContent = "Error";
      }
      setTimeout(() => { btnRefresh.textContent = prev; }, 900);
    });
  }

  if (btnCopyUrl) {
    btnCopyUrl.addEventListener("click", async () => {
      const url = `${location.protocol}//${location.host}`;
      try {
        await navigator.clipboard.writeText(url);
        btnCopyUrl.textContent = "Copiado";
        setTimeout(() => { btnCopyUrl.textContent = "Copiar enlace"; }, 1400);
      } catch (_) {
        btnCopyUrl.textContent = url;
      }
    });
  }

  async function connectSignalR() {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(apiUrl("/hubs/game"))
      .withAutomaticReconnect()
      .build();

    connection.on("PlayerPoseUpdated", (pose) => {
      const n = normalizePose(pose);
      if (!n || n.playerId !== PLAYER_ID) return;
      applyPlayerPose(n);
    });

    connection.on("MarkersChanged", async (zoneId, payload) => {
      if (zoneId && zoneId !== ZONE_ID) return;
      if (Array.isArray(payload)) {
        markers = payload;
        renderList();
        scheduleDraw(true);
      } else {
        await loadMarkers();
      }
    });

    connection.onreconnecting(() => setStatus("Reconectando…", "pending"));
    connection.onreconnected(() => setStatus("En vivo", "ok"));

    await connection.start();
    await connection.invoke("JoinZone", ZONE_ID);
    setStatus("En vivo", "ok");

    // Fallback poll if SignalR payloads arrive with unexpected shape
    setInterval(() => { loadPlayer().catch(() => {}); }, 2000);
  }

  async function boot() {
    try {
      setStatus("Conectando…", "pending");
      await loadZone();
      await loadMarkers();
      await loadPlayer();
      resizeCanvas();
      bindMapInput();
      window.addEventListener("resize", resizeCanvas);
      rafId = requestAnimationFrame(loop);
      await connectSignalR();
    } catch (err) {
      console.error(err);
      setStatus("Sin conexión", "err");
    }
  }

  boot();
})();
