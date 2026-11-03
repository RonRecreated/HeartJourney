import { test } from 'node:test';
import assert from 'node:assert/strict';
import { validatePurchase } from '../../supabase/functions/verify-google-play-purchase/google-play.ts';

const purchase = (state = 'PURCHASED') => ({
    purchaseStateContext: { purchaseState: state },
    productLineItem: [{ productId: 'journey_unlock', productOfferDetails: { consumptionState: 'CONSUMPTION_STATE_YET_TO_BE_CONSUMED' } }],
    acknowledgementState: 'ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED'
});
test('completed, pending and canceled states remain distinct', () => {
    for (const state of ['PURCHASED', 'PENDING', 'CANCELLED'])
        assert.equal(validatePurchase(purchase(state), 'journey_unlock', false).state, state);
});
test('wrong products and unknown purchase states fail verification', () => {
    assert.throws(() => validatePurchase(purchase(), 'another_journey', false));
    assert.throws(() => validatePurchase(purchase('UNKNOWN'), 'journey_unlock', false));
    assert.throws(() => validatePurchase({}, 'journey_unlock', false));
});
test('test purchases require explicit server configuration', () => {
    const item = { ...purchase(), testPurchaseContext: {} };
    assert.throws(() => validatePurchase(item, 'journey_unlock', false));
    assert.equal(validatePurchase(item, 'journey_unlock', true).state, 'PURCHASED');
});
test('consumed purchases and rental products cannot unlock a permanent journey', () => {
    const item = purchase();
    item.productLineItem[0].productOfferDetails.consumptionState = 'CONSUMPTION_STATE_CONSUMED';
    assert.throws(() => validatePurchase(item, 'journey_unlock', false));
    item.productLineItem[0].productOfferDetails = { rentOfferDetails: {} };
    assert.throws(() => validatePurchase(item, 'journey_unlock', false));
});
