import http from "k6/http";
import { check } from "k6";

export const options = {
  vus: 20,
  iterations: 5000,
};

export default function () {
  const url = "http://localhost:5154/api/audit";
  const payload = JSON.stringify({
    timestamp: new Date().toISOString(),
    userId: `user-${__VU % 50}`,
    actionType: `action-${__VU % 10}`,
    entityId: `entity-${__ITER}`,
    metadata: { foo: "bar", n: __ITER },
  });

  const res = http.post(url, payload, { headers: { "Content-Type": "application/json" } });
  check(res, { "202/accepted": (r) => r.status === 202 });
}
