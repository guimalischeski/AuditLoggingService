import http from "k6/http";
import { Trend } from "k6/metrics";
import { check } from "k6";

export const options = {
  vus: 1,
  iterations: 20,
};

const getLatency = new Trend("get_latency");

export default function () {
  const userId = "user-1";
  const actionType = "action-1";

  const url =
    `http://localhost:5154/api/audit?userId=${userId}&actionType=${actionType}&page=1&pageSize=50`;

  const res = http.get(url);

  check(res, {
    "status is 200": (r) => r.status === 200,
  });

  getLatency.add(res.timings.duration);
}
