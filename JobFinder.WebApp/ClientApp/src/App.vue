<template>
  <div className="head">
    <h1>Welcome to Job Finder!</h1>
  </div>
  <div className="container">

    <div className="inputBox">
      <ul>
        <li v-for="(source, index) in sources" :key="index" @click="setUrl(source)" :className="isUrlActive(source) ? 'active' : ''">{{ source }}</li>
      </ul>
      <CriteriaInput :changeSpeciality="changeSpeciality" :changeArea="changeArea" :findJobs="findJobs"></CriteriaInput>
      <button @click="findJobs()" style="margin-inline: 0.5rem;">Find Job</button>
    </div>

    <div className="listBox">      
      <h3 v-if="loading">Loading...</h3>
      <div v-for="(el, index) in jobs" :key="index">
        <a :href="el['link']" target="_blank">
          {{el['title']}}
        </a>
      </div>

    </div>
  </div>
</template>

<script>
import CriteriaInput from './components/CriteriaInput.vue'

export default {
components: {
  CriteriaInput
},

  data() {
    return {
      sources: [
        'linkedIn',
        'rabotaBy'
      ],
      jobs: [],
      urls: [
        {
          name: 'linkedIn',
          path: 'https://www.linkedin.com/search/results/all/?',
          active: false
        },
        {
          name: 'rabotaBy',
          path: 'https://rabota.by/search/vacancy/?',
          active: false
        }
      ],
      speciality: '',
      area: '',
      loading: false
    }
  },

  methods: {
    async findJobs() {
      var loadedJobs = [];
      this.jobs = [];
      
      this.urls.forEach(async url => {
        if(url.active)
        {
          this.loading = true;

          loadedJobs = await fetch(
            'https://localhost:7150/Jobs/GetList',
            {
              body: JSON.stringify({
                url: url.path,
                speciality: this.speciality,
                area: this.area,
                source: url.name
              }),
              method: 'POST',
              headers: {
                'Content-Type' : 'application/json'            
              }
            }
          ).then(response => response.json());

          loadedJobs.forEach(job => this.jobs.push(job));          
        }

        this.loading = false
      });
    },

    changeSpeciality(value) {
      this.speciality = String(value).trim();
    },

    changeArea(value) {
      this.area = String(value).trim();
    },

    setUrl(value) {
      this.urls.forEach(url => {
        if (url['name'] === value) {
          url['active'] = !url['active'];
          return;
        }
      })
    },

    isUrlActive(value) {
      const active = this.urls.find(x => x['name'] == value)['active'];      
      return active;
    }
  }  
}
</script>

<style scoped>
  div {
    display: flex; 
    flex-wrap: wrap;
  }

  .head {
    flex-direction: column; 
    align-items: center;
  }

  .container {
    flex-direction: row;
    max-width: 100%;
  }

  .inputBox {
    width:16%; 
    flex-direction: column; 
    align-content: flex-start;
  }

  .listBox {
    max-width: 83%; 
    flex-direction: row;
    padding-top: 8px;
    justify-content: start;
    gap: 2px;
  }

  a {
      display: block;
      text-decoration:none;
      color:black;
      padding: 3px;
      margin: 2px;
      background-color: gray;
  }

  a:hover {
    color:lightgray;
    background: #333;
  }

  a:visited {
    color:darkgray;
    font-style: italic;
  }

  h1, h3 {
    color:rgb(180, 29, 29);
    font-weight: bold;
    background-color: #1A032D;
    border-radius:10px;
    width: fit-content;
    padding: 5px;
    text-align: center;
  }

  ul {
        margin: 5px;
        list-style: none;
        padding: 0;
        border-radius: 3px;
        border: 3px solid #000
    }

    li {
        display: block;
        background: #1A032D;
        color: #fff;
        padding: 20px 0;
        text-align: center;
    }   

    li:hover, li.active {
        background: hsl(273, 88%, 23%);
        cursor: pointer;
    }
</style>
