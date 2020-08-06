import {Mutation, Query, SingleQuery} from './queries';

/**
 * @customfunction
 * @param invocation Custom function handler
 */
export function activeBranch(invocation : CustomFunctions.StreamingInvocation<string>) : void {
  const query = `query { activeBranch }`;

  Query(query, data => data.activeBranch.toString(), invocation);
}

/**
 * @customfunction
 * @param name Item name
 * @param invocation Custom function handler
 */
export function itemId(name : string, invocation : CustomFunctions.StreamingInvocation<string>) : void {

  const query = `query {
    itemInfo(name : "${name}") { itemId } 
  }`;

  SingleQuery(query, data => data.itemInfo.itemId.toString())
      .then(res => {
        let id = 0;  
        try {
            id = Number(res);
        }
        catch (e) {
           invocation.setResult(e); 
        }
        
        if (id == 0)
        {
          const mutation = `mutation {
            addItem( name : "${name}" )
          }`;
          
          Mutation(mutation).catch(e => invocation.setResult(e));
        }
        
        Query(query, data => data.itemInfo.itemId.toString(), invocation, result => Number(result) == 0);
      })
      .catch(e => invocation.setResult(e));
}

/**
 * @customfunction
 * @param {string} name Item name
 */
export async function itemIdSingle(name : string) : Promise<any> {
  
  const query = `query {
    itemInfo(name : "${name}") { itemId } 
  }`;

  let id = 0;
  let res = await SingleQuery(query, data => data.itemInfo.itemId.toString());
  try {
    id = Number(res);
  }
  catch (e) {
    return res;
  }

  if (id == 0)
  {
    const mutation = `mutation {
        addItem( name : "${name}" )
      }`;
    await Mutation(mutation);
  }
  id = Number(await SingleQuery(query, data => data.itemInfo.itemId.toString()));
  return id;
}

