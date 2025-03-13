import { expect } from '@jest/globals';
import { DotNet } from "../src/Microsoft.JSInterop";

describe('findObjectMember', () => {
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
        const result = DotNet.findObjectMember('a.b.c', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'c', kind: 'data' });
    });

    test('resolves function member', () => {
        const result = DotNet.findObjectMember('a.b.d', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'd', kind: 'function' });
    });

    test('resolves constructor function member', () => {
        const result = DotNet.findObjectMember('a.b.e', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'e', kind: 'function' });
    });

    test('resolves property member', () => {
        const result = DotNet.findObjectMember('a.b', objectId);
        expect(result).toEqual({ parent: { b: { c: 42, d: expect.any(Function), e: expect.any(Function) } }, memberName: 'b', kind: 'property' });
    });

    test('resolves undefined member', () => {
        const result = DotNet.findObjectMember('a.b.c.f', objectId);
        expect(result).toEqual({ parent: { c: 42, d: expect.any(Function), e: expect.any(Function) }, memberName: 'f', kind: 'undefined' });
    });

    test('throws error for non-existent instance ID', () => {
        expect(() => DotNet.findObjectMember('a.b.c', 999)).toThrow('JS object instance with ID 999 does not exist (has it been disposed?).');
    });
});

describe('findObjectMember with window object', () => {
    let windowObjectId: number;

    beforeAll(() => {
        // Creating JS object reference for window object
        windowObjectId = DotNet.createJSObjectReference(window)["__jsObjectId"];
    });

    test('resolves document.title', () => {
        document.title = 'Test Title';
        const result = DotNet.findObjectMember('document.title', windowObjectId);
        expect(result).toEqual({ parent: document, memberName: 'title', kind: 'data' });
    });

    test('resolves window.location', () => {
        const result = DotNet.findObjectMember('location', windowObjectId);
        expect(result).toEqual({ parent: expect.any(Object), memberName: 'location', kind: 'property' });
    });

    test('resolves window.alert', () => {
        const result = DotNet.findObjectMember('alert', windowObjectId);
        expect(result).toEqual({ parent: expect.any(Object), memberName: 'alert', kind: 'function' });
    });

    test('resolves undefined for non-existent window member', () => {
        const result = DotNet.findObjectMember('nonExistentMember', windowObjectId);
        expect(result).toEqual({ parent: null, memberName: 'nonExistentMember', kind: 'undefined' });
    });
});