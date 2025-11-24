// guidToUser.js
export const KEYCLOAK_URL = "http://localhost:8082";
export const REALM = "chatapp";
export const ADMIN_USERNAME = "khai";
export const ADMIN_PASSWORD = "123";

/**
 * Lấy admin token từ Keycloak
 */
export async function getAdminToken() {
    const params = new URLSearchParams();
    params.append("client_id", "admin-cli");
    params.append("username", ADMIN_USERNAME);
    params.append("password", ADMIN_PASSWORD);
    params.append("grant_type", "password");

    const res = await fetch(`${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: params.toString()
    });

    if (!res.ok) throw new Error(`Cannot get admin token: ${res.status}`);
    const data = await res.json();
    return data.access_token;
}

/**
 * Lấy username từ guid
 * @param {string} guid - guid của user
 * @param {string} adminToken - token admin
 * @returns {Promise<string>} username
 */
export async function getUsernameFromGuid(guid, adminToken) {
    try {
        const res = await fetch(`${KEYCLOAK_URL}/admin/realms/${REALM}/users/${guid}`, {
            headers: {
                "Authorization": `Bearer ${adminToken}`,
                "Content-Type": "application/json"
            }
        });

        if (!res.ok) return guid;
        const data = await res.json();
        return data.username || guid;
    } catch (err) {
        console.error(err);
        return guid;
    }
}

/**
 * Chuyển mảng guid sang username
 * @param {Array<string>} guids
 * @returns {Promise<Object>} object mapping guid -> username
 */
export async function mapGuidsToUsernames(guids) {
    try {
        const res = await fetch("https://localhost:4200/api/users/map-guids", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(guids)
        });
        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();
        return data; // { guid: username, ... }
    } catch (err) {
        console.error(err);
        const fallback = {};
        guids.forEach(g => fallback[g] = g);
        return fallback;
    }
}

