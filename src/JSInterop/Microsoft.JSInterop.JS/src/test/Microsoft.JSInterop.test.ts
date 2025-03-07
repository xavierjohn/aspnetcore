import { expect } from '@jest/globals';
import { DotNet } from "../src/Microsoft.JSInterop";

describe('resolveObjectMember', () => {
    let objectId: number;

    beforeAll(() => {
        objectId = DotNet.createJSObjectReference({
            a: {
                b: {
                    c: 42,
                    d: function () { return 'hello'; },
                    e: class { constructor() { } }
                }
            }
        })["__jsObjectId"];
    });

    test('resolves data member', () => {
        const result = DotNet.resolveObjectMember('a.b.c', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'c', kind: 'data' });
    });

    test('resolves function member', () => {
        const result = DotNet.resolveObjectMember('a.b.d', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'd', kind: 'function' });
    });

    test('resolves constructor function member', () => {
        const result = DotNet.resolveObjectMember('a.b.e', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'e', kind: 'function' });
    });

    test('resolves property member', () => {
        const result = DotNet.resolveObjectMember('a.b', objectId);
        expect(result).toEqual({ parent: { b: { c: 42, d: expect.any(Function), e: expect.any(Function) } }, memberName: 'b', kind: 'property' });
    });

    test('resolves undefined member', () => {
        const result = DotNet.resolveObjectMember('a.b.c.f', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'f', kind: 'undefined' });
    });

    test('throws error for non-existent instance ID', () => {
        expect(() => DotNet.resolveObjectMember('a.b.c', 999)).toThrow('JS object instance with ID 999 does not exist (has it been disposed?).');
    });
});

describe('resolveObjectMember with window object', () => {
    let windowObjectId: number;

    beforeAll(() => {
        // Creating JS object reference for window object
        windowObjectId = DotNet.createJSObjectReference(window)["__jsObjectId"];
    });

    test('resolves document.title', () => {
        document.title = 'Test Title';
        const result = DotNet.resolveObjectMember('document.title', windowObjectId);
        expect(result).toEqual({ parent: document, memberName: 'title', kind: 'data' });
    });

    test('resolves window.location', () => {
        const result = DotNet.resolveObjectMember('location', windowObjectId);
        expect(result).toEqual({ parent: expect.any(Object), memberName: 'location', kind: 'property' });
    });

    test('resolves window.alert', () => {
        const result = DotNet.resolveObjectMember('alert', windowObjectId);
        expect(result).toEqual({ parent: expect.any(Object), memberName: 'alert', kind: 'function' });
    });

    test('resolves undefined for non-existent window member', () => {
        const result = DotNet.resolveObjectMember('nonExistentMember', windowObjectId);
        expect(result).toEqual({ parent: null, memberName: 'nonExistentMember', kind: 'undefined' });
    });
});