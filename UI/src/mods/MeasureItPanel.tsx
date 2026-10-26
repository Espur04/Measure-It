import { bindValue, trigger, useValue } from "cs2/api";
import { useEffect, useRef, useState } from "react";
import type { MouseEvent as ReactMouseEvent } from "react";
import { useLocalization } from "cs2/l10n";
import logoSvg from "./logo.svg?raw";
import styles from "./MeasureItPanel.module.scss";

const GROUP = "measureItCS2";

const toolActive$ = bindValue<boolean>(GROUP, "toolActive", false);
const unitOfDistance$ = bindValue<number>(GROUP, "unitOfDistance", 0);
const unitOfHeight$ = bindValue<number>(GROUP, "unitOfHeight", 0);
const unitOfSlope$ = bindValue<number>(GROUP, "unitOfSlope", 0);
const unitOfDirection$ = bindValue<number>(GROUP, "unitOfDirection", 0);
const unitOfArea$ = bindValue<number>(GROUP, "unitOfArea", 0);
const panelPlaced$ = bindValue<boolean>(GROUP, "panelPlaced", false);
const panelX$ = bindValue<number>(GROUP, "panelX", 0);
const panelY$ = bindValue<number>(GROUP, "panelY", 0);
const snapToNodes$ = bindValue<boolean>(GROUP, "snapToNodes", true);
const forceGroundHeight$ = bindValue<boolean>(GROUP, "forceGroundHeight", false);
const lockCurrentHeight$ = bindValue<boolean>(GROUP, "lockCurrentHeight", false);
const measureColor$ = bindValue<number>(GROUP, "measureColor", 2);

export interface MeasurementRow {
    index: number;
    relief: string;
    length: string;
    distance: string;
    slope: string;
    direction: string;
}

export interface MeasurementSummary {
    pointCount: number;
    elevation: string;
    netElevation: string;
    totalAscent: string;
    totalDescent: string;
    minElevation: string;
    maxElevation: string;
    planDistance: string;
    area: string;
    areaAvailable: boolean;
    areaPreview: boolean;
    totalDistance: string;
    straightLineDistance: string;
    distanceUnitSymbol: string;
    heightUnitSymbol: string;
    areaUnitSymbol: string;
    slopeUnitSymbol: string;
    directionUnitSymbol: string;
}

const DEFAULT_SUMMARY: MeasurementSummary = {
    pointCount: 0,
    elevation: "0",
    netElevation: "0",
    totalAscent: "0",
    totalDescent: "0",
    minElevation: "0",
    maxElevation: "0",
    planDistance: "0",
    area: "—",
    areaAvailable: false,
    areaPreview: false,
    totalDistance: "0",
    straightLineDistance: "0",
    distanceUnitSymbol: "m",
    heightUnitSymbol: "m",
    areaUnitSymbol: "m²",
    slopeUnitSymbol: "°",
    directionUnitSymbol: "\u00b0",
};

const segmentsJson$ = bindValue<string>(GROUP, "segmentsJson", "[]");
const summaryJson$ = bindValue<string>(GROUP, "summaryJson", JSON.stringify(DEFAULT_SUMMARY));

const DISTANCE_OPTIONS = ["Unit.Meters", "Unit.Kilometers", "Unit.Feet", "Unit.Miles"];
const AREA_OPTIONS = ["Area.SquareMeters", "Area.Hectares", "Area.SquareKilometers", "Area.SquareFeet", "Area.Acres"];
const SLOPE_OPTIONS = ["Unit.Degrees", "Unit.Percentage"];
const DIRECTION_OPTIONS = ["Unit.Degrees", "Unit.Cardinal"];
const COLOR_OPTIONS = [
    { value: 2, key: "Color.Red", fallback: "Red", color: "#FF2626" },
    { value: 0, key: "Color.Yellow", fallback: "Yellow", color: "#FFD900" },
    { value: 1, key: "Color.Magenta", fallback: "Magenta", color: "#FF00FF" },
    { value: 3, key: "Color.Green", fallback: "Green", color: "#33FF33" },
    { value: 4, key: "Color.Cyan", fallback: "Cyan", color: "#00E5FF" },
    { value: 5, key: "Color.White", fallback: "White", color: "#FFFFFF" },
    { value: 6, key: "Color.Orange", fallback: "Orange", color: "#FF8000" },
];


function safeParse<T>(json: string, fallback: T): T {
    try {
        return JSON.parse(json) as T;
    } catch {
        return fallback;
    }
}

const PANEL_WIDTH = 480;
const PANEL_MAX_HEIGHT_FRACTION = 0.78;


interface PanelSpot {
    left: number;
    top: number;
}


interface PanelFraction {
    x: number;
    y: number;
}

function clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), Math.max(min, max));
}


export function MeasureItPanel() {
    const localization = useLocalization();
    const text = (key: string, fallback: string) => localization.translate(`MeasureItCS2.UI.${key}`, fallback) ?? fallback;
    const optionText = (key: string, fallback: string) => localization.translate(`MeasureItCS2.${key}`, fallback) ?? fallback;
    const toolActive = useValue(toolActive$);
    const unitOfDistance = useValue(unitOfDistance$);
    const unitOfHeight = useValue(unitOfHeight$);
    const unitOfSlope = useValue(unitOfSlope$);
    const unitOfDirection = useValue(unitOfDirection$);
    const unitOfArea = useValue(unitOfArea$);
    const segmentsJson = useValue(segmentsJson$);
    const summaryJson = useValue(summaryJson$);
    const panelPlaced = useValue(panelPlaced$);
    const panelX = useValue(panelX$);
    const panelY = useValue(panelY$);
    const snapToNodes = useValue(snapToNodes$);
    const forceGroundHeight = useValue(forceGroundHeight$);
    const lockCurrentHeight = useValue(lockCurrentHeight$);
    const measureColor = useValue(measureColor$);

    const distanceOptions = DISTANCE_OPTIONS.map((key, value) => ({ value, label: optionText(key, ["Meters", "Kilometers", "Feet", "Miles"][value]) }));
    const areaOptions = AREA_OPTIONS.map((key, value) => ({ value, label: optionText(key, ["m²", "ha", "km²", "ft²", "Acres"][value]) }));
    const slopeOptions = SLOPE_OPTIONS.map((key, value) => ({ value, label: optionText(key, ["Degrees", "Percentage"][value]) }));
    const directionOptions = DIRECTION_OPTIONS.map((key, value) => ({ value, label: optionText(key, ["Degrees", "Cardinal"][value]) }));

    const segments: MeasurementRow[] = safeParse(segmentsJson, []);
    const summary: MeasurementSummary = safeParse(summaryJson, DEFAULT_SUMMARY);
    const rawRows = segments.map((row) =>
        [row.index, row.distance, row.relief, row.slope, row.direction]
            .map((value) => String(value).replace(/,/g, "."))
            .join("_"),
    );
    const rawSegmentText = rawRows.join(";");

    const visible = toolActive;

    const anchorRef = useRef<HTMLDivElement>(null);
    const [dragSpot, setDragSpot] = useState<PanelSpot | null>(null);
    const [pendingSpot, setPendingSpot] = useState<PanelFraction | null>(null);
    const [copyState, setCopyState] = useState<"idle" | "copied" | "failed">("idle");
    const stopDragRef = useRef<(() => void) | null>(null);

    const copyRaw = () => {
        if (segments.length === 0) {
            return;
        }

        try {
            trigger(GROUP, "copyRaw", rawSegmentText);
            setCopyState("copied");
        } catch {
            setCopyState("failed");
        }
    };

    useEffect(() => {
        if (copyState === "idle") {
            return undefined;
        }
        const timeout = setTimeout(() => setCopyState("idle"), 1600);
        return () => clearTimeout(timeout);
    }, [copyState]);

    useEffect(() => {
        if (!pendingSpot) {
            return undefined;
        }

        const arrived = Math.abs(panelX - pendingSpot.x) < 0.0005 && Math.abs(panelY - pendingSpot.y) < 0.0005;
        if (arrived) {
            setPendingSpot(null);
            return undefined;
        }

        const timeout = setTimeout(() => setPendingSpot(null), 2000);
        return () => clearTimeout(timeout);
    }, [panelX, panelY, pendingSpot]);

    useEffect(() => () => stopDragRef.current?.(), []);

    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const panelMaxHeight = viewportHeight * PANEL_MAX_HEIGHT_FRACTION;
    const spotFromFraction = (x: number, y: number): PanelSpot => ({
        left: clamp(x * viewportWidth, 0, viewportWidth - PANEL_WIDTH),
        top: clamp(y * viewportHeight, 0, viewportHeight - panelMaxHeight),
    });

    const defaultSpot: PanelSpot = {
        left: clamp((225 / 1920) * viewportWidth, 0, viewportWidth - PANEL_WIDTH),
        top: clamp((150 / 1080) * viewportHeight, 0, viewportHeight - panelMaxHeight),
    };

    let spot: PanelSpot | null = null;
    if (dragSpot) {
        spot = dragSpot;
    } else if (pendingSpot) {
        spot = spotFromFraction(pendingSpot.x, pendingSpot.y);
    } else if (panelPlaced) {
        spot = spotFromFraction(panelX, panelY);
    }

    const onHeaderMouseDown = (event: ReactMouseEvent) => {
        const anchor = anchorRef.current;
        if (event.button !== 0 || !anchor) {
            return;
        }
        event.preventDefault();

        const rect = anchor.getBoundingClientRect();
        const startX = event.clientX;
        const startY = event.clientY;
        const clampSpot = (left: number, top: number): PanelSpot => ({
            left: clamp(left, 0, window.innerWidth - rect.width),
            top: clamp(top, 0, window.innerHeight - rect.height),
        });

        let last = clampSpot(rect.left, rect.top);
        let moved = false;
        setDragSpot(last);

        const onMove = (move: MouseEvent) => {
            const dx = move.clientX - startX;
            const dy = move.clientY - startY;
            if (!moved && Math.abs(dx) + Math.abs(dy) > 2) {
                moved = true;
            }
            last = clampSpot(rect.left + dx, rect.top + dy);
            setDragSpot(last);
        };

        const stop = () => {
            window.removeEventListener("mousemove", onMove);
            window.removeEventListener("mouseup", onUp);
            stopDragRef.current = null;
        };

        const onUp = () => {
            stop();
            setDragSpot(null);

            if (!moved) {
                return;
            }

            const fraction: PanelFraction = {
                x: last.left / window.innerWidth,
                y: last.top / window.innerHeight,
            };
            setPendingSpot(fraction);
            trigger(GROUP, "setPanelPosition", fraction.x, fraction.y);
        };

        stopDragRef.current = stop;
        window.addEventListener("mousemove", onMove);
        window.addEventListener("mouseup", onUp);
    };

    return (
        <div
            ref={anchorRef}
            className={`${styles.anchor} ${spot ? "" : styles.anchorDefault}`}
            style={{ left: `${(spot ?? defaultSpot).left}px`, top: `${(spot ?? defaultSpot).top}px` }}
        >
            <div className={`${styles.panel} ${visible ? "" : styles.hidden}`}>
                <div className={styles.header} onMouseDown={onHeaderMouseDown}>
                    <div className={styles.logo} aria-hidden="true" dangerouslySetInnerHTML={{ __html: logoSvg }} />
                    <span className={styles.title}>Measure It!</span>
                </div>
                <div className={styles.panelContent}>
                <SectionHeader title={text("UnitsSetup", "Units setup")} />
                <div className={styles.unitsRow}>
                    <UnitDropdown
                        label={text("Distance", "Distance")}
                        value={unitOfDistance}
                        options={distanceOptions}
                        onChange={(value) => trigger(GROUP, "setUnitOfDistance", value)}
                        active={toolActive}
                    />
                    <UnitDropdown
                        label={text("Height", "Height")}
                        value={unitOfHeight}
                        options={distanceOptions}
                        onChange={(value) => trigger(GROUP, "setUnitOfHeight", value)}
                        active={toolActive}
                    />
                </div>

                <div className={styles.unitsRow}>
                    <UnitDropdown
                        label={text("Slope", "Slope")}
                        value={unitOfSlope}
                        options={slopeOptions}
                        onChange={(value) => trigger(GROUP, "setUnitOfSlope", value)}
                        active={toolActive}
                    />
                    <UnitDropdown
                        label={text("Direction", "Direction")}
                        value={unitOfDirection}
                        options={directionOptions}
                        onChange={(value) => trigger(GROUP, "setUnitOfDirection", value)}
                        active={toolActive}
                    />
                    <UnitDropdown
                        label={text("Area", "Area")}
                        value={unitOfArea}
                        options={areaOptions}
                        onChange={(value) => trigger(GROUP, "setUnitOfArea", value)}
                        active={toolActive}
                    />
                </div>

                <div className={styles.optionsRow}>
                    <Checkbox
                        checked={snapToNodes}
                        onChange={(checked) => trigger(GROUP, "setSnapToNodes", checked)}
                        label={text("SnapShort", "Snap")}
                        title={text("SnapTooltip", "Snap placed points to nearby network nodes")}
                    />
                    <Checkbox
                        checked={forceGroundHeight}
                        onChange={(checked) => trigger(GROUP, "setForceGroundHeight", checked)}
                        label={text("GroundShort", "Ground")}
                        title={text("GroundTooltip", "Place points at terrain height")}
                    />
                    <Checkbox
                        checked={lockCurrentHeight}
                        onChange={(checked) => trigger(GROUP, "setLockCurrentHeight", checked)}
                        label={text("LockShort", "Lock")}
                        title={text("LockTooltip", "Keep new points at the last point's height")}
                    />
                    <div className={styles.colorSwatches} role="group" aria-label={text("LineColor", "Line color")}>
                        {COLOR_OPTIONS.map((option) => {
                            const label = optionText(option.key, option.fallback);
                            return (
                                <button
                                    key={option.value}
                                    type="button"
                                    className={`${styles.colorSwatch} ${measureColor === option.value ? styles.colorSwatchSelected : ""}`}
                                    style={{ backgroundColor: option.color }}
                                    title={label}
                                    aria-label={label}
                                    aria-pressed={measureColor === option.value}
                                    onClick={() => trigger(GROUP, "setMeasureColor", option.value)}
                                />
                            );
                        })}
                    </div>
                </div>


                <div className={`${styles.metricsBlock} ${summary.areaPreview ? styles.metricsPreview : ""}`}>
                    <div className={styles.metricsRow}>
                        <Metric label={text(summary.areaPreview ? "AreaPreview" : "Area", summary.areaPreview ? "Area preview" : "Area")} value={summary.areaAvailable ? summary.area : "—"} unit={summary.areaUnitSymbol} />
                        <Metric label={text("Distance", "Distance")} value={summary.totalDistance} unit={summary.distanceUnitSymbol} />
                        <Metric label={text("NetElevation", "Net elevation")} value={summary.netElevation} unit={summary.heightUnitSymbol} />
                    </div>
                    <div className={styles.metricsRow}>
                        <Metric label={text("TotalAscent", "Total ascent")} value={summary.totalAscent} unit={summary.heightUnitSymbol} />
                        <Metric label={text("PlanDistance", "Plan distance")} value={summary.planDistance} unit={summary.distanceUnitSymbol} />
                        <Metric label={text("TotalDescent", "Total descent")} value={summary.totalDescent} unit={summary.heightUnitSymbol} />
                    </div>
                </div>
                <SectionHeader title={text("Segments", "Segments")} />
                <div className={styles.segmentTable}>
                    <div className={`${styles.segmentRow} ${styles.segmentHeaderRow}`}>
                        <span className={styles.colIndex}>#</span>
                <span className={styles.colValue}>{`${text("SegmentDistance", "DIST.")} ${summary.distanceUnitSymbol}`}</span>
                <span className={styles.colValue}>{`${text("SegmentRise", "RISE")} ${summary.heightUnitSymbol}`}</span>
                <span className={styles.colValue}>{`${text("SegmentGrade", "GRADE")} ${summary.slopeUnitSymbol}`}</span>
                <span className={styles.colValue}>{`${text("SegmentBearing", "BEARING")} ${summary.directionUnitSymbol}`}</span>
                    </div>
                    <div className={styles.segmentList}>
                        {segments.map((row) => (
                            <div className={styles.segmentRow} key={row.index}>
                                <span className={styles.colIndex}>{row.index}</span>
                                <span className={styles.colValue}>{row.distance}</span>
                                <span className={styles.colValue}>{row.relief}</span>
                                <span className={styles.colValue}>{row.slope}</span>
                                <span className={styles.colValue}>{row.direction}</span>
                            </div>
                        ))}
                    </div>
                </div>

                <div className={styles.actions}>
                    <button
                        type="button"
                        className={styles.copyButton}
                        disabled={segments.length === 0}
                        onClick={() => void copyRaw()}
                    >
                        {copyState === "copied"
                            ? text("Copied", "Copied")
                            : copyState === "failed"
                                ? text("CopyFailed", "Copy failed")
                                : text("CopyRaw", "Copy RAW")}
                    </button>
                    <button type="button" className={styles.clearButton} onClick={() => trigger(GROUP, "clearPoints")}>
                        {text("Clear", "Clear")}
                    </button>
                </div>
                </div>
            </div>
        </div>
    );
}

function SectionHeader({ title }: { title: string }) {
    return (
        <div className={styles.sectionHeader}>
            <span>{title}</span>
        </div>
    );
}

function Metric({
    label,
    value,
    unit,
}: {
    label: string;
    value: string;
    unit: string;
}) {
    return (
        <div className={styles.metric}>
            <span className={styles.metricLabel}>{label}</span>
            <span className={styles.metricValue}>
                {value}<small>{unit}</small>
            </span>
        </div>
    );
}


function Checkbox({ checked, onChange, label, title }: { checked: boolean; onChange: (checked: boolean) => void; label: string; title: string }) {
    return (
        <div className={styles.checkboxLabel} title={title} onClick={() => onChange(!checked)}>
            <div className={`${styles.checkboxBox} ${checked ? styles.checkboxBoxChecked : ""}`}>
                {checked && <span className={styles.checkmark} aria-hidden="true" />}
            </div>
            <span>{label}</span>
        </div>
    );
}

function UnitDropdown({
    label,
    value,
    options,
    onChange,
    className,
    active,
}: {
    label: string;
    value: number;
    options: { value: number; label: string }[];
    onChange: (value: number) => void;
    className?: string;
    active: boolean;
}) {
    const current = options.find((o) => o.value === value) ?? options[0];
    const rootRef = useRef<HTMLDivElement>(null);
    const [open, setOpen] = useState(false);

    useEffect(() => {
        if (!open) {
            return undefined;
        }

        const closeOnOutsideClick = (event: MouseEvent) => {
            if (!rootRef.current?.contains(event.target as Node)) {
                setOpen(false);
            }
        };
        const closeOnEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                setOpen(false);
            }
        };

        window.addEventListener("mousedown", closeOnOutsideClick);
        window.addEventListener("keydown", closeOnEscape);
        return () => {
            window.removeEventListener("mousedown", closeOnOutsideClick);
            window.removeEventListener("keydown", closeOnEscape);
        };
    }, [open]);

    useEffect(() => {
        if (!active) {
            setOpen(false);
        }
    }, [active]);

    if (!active) {
        return (
            <div className={`${styles.unitDropdown} ${className ?? ""}`} ref={rootRef}>
                <span className={styles.unitLabel}>{label}</span>
                <span className={styles.selectStatic}>{current.label}</span>
            </div>
        );
    }

    return (
        <div className={`${styles.unitDropdown} ${className ?? ""}`} ref={rootRef}>
            <span className={styles.unitLabel}>{label}</span>
            <button
                type="button"
                className={`${styles.selectButton} ${open ? styles.selectButtonOpen : ""}`}
                aria-expanded={open}
                onClick={() => setOpen(!open)}
            >
                <span>{current.label}</span>
                <span className={`${styles.selectChevron} ${open ? styles.selectChevronOpen : ""}`} aria-hidden="true" />
            </button>
            {open && (
                <div className={styles.dropdownList}>
                    {options.map((option) => (
                        <button
                            key={option.value}
                            type="button"
                            className={`${styles.dropdownOption} ${option.value === value ? styles.dropdownOptionSelected : ""}`}
                            onClick={() => {
                                onChange(option.value);
                                setOpen(false);
                            }}
                        >
                            <span>{option.label}</span>
                            {option.value === value && <span className={styles.optionCheck} aria-hidden="true" />}
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
}
