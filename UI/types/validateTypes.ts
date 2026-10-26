import { ModRegistrar } from "cs2/modding";

async function validateExportTypes() {
    let isIndexFileValid: { 'default': ModRegistrar } = await import('../src/index');
    return isIndexFileValid;
}